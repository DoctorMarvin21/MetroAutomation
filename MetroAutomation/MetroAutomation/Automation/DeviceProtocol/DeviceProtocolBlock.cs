﻿using LiteDB;
using MahApps.Metro.Controls.Dialogs;
using MetroAutomation.Calibration;
using MetroAutomation.Controls;
using MetroAutomation.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MetroAutomation.Automation
{
    [Serializable]
    public class DeviceProtocolBlock : INotifyPropertyChanged
    {
        [NonSerialized]
        private bool isEnabled;
        private string name;

        [NonSerialized]
        private DeviceProtocolItem itemInProgress;

        public DeviceProtocolBlock()
        {
            BindableItems.CollectionChanged += (s, e) =>
            {
                if (e.NewItems?.Count > 0)
                {
                    foreach (DeviceProtocolItem item in e.NewItems)
                    {
                        item.PropertyChanged += (sp, ep) =>
                        {
                            if (ep.PropertyName == nameof(DeviceProtocolItem.IsSelected))
                            {
                                OnPropertyChanged(nameof(IsSelected));
                            }
                            else if (ep.PropertyName == nameof(DeviceProtocolItem.Status))
                            {
                                OnPropertyChanged(nameof(Status));
                            }
                        };
                    }
                }

                OnPropertyChanged(nameof(IsSelected));
                OnPropertyChanged(nameof(Status));
            };
        }

        [BsonIgnore]
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        [BsonIgnore]
        [field: NonSerialized]
        public DeviceProtocol Owner { get; private set; }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                OnPropertyChanged();
                UpdateDisplayedName();
            }
        }

        [BsonIgnore]
        [field: NonSerialized]
        public string DisplayedName { get; set; }

        public AutomationMode AutomationMode { get; set; }

        public IAsyncCommand RemoveFromOwner => new AsyncCommandHandler(Remove);

        [BsonIgnore]
        public bool IsEnabled
        {
            get
            {
                return isEnabled;
            }
            private set
            {
                isEnabled = value;
                OnPropertyChanged();
                UpdateDisplayedName();
            }
        }

        [BsonIgnore]
        public LedState Status
        {
            get
            {
                if (BindableItems.Count == 0)
                {
                    return LedState.Idle;
                }
                else
                {
                    if (BindableItems.Any(x => x.Status == LedState.Warn))
                    {
                        return LedState.Warn;
                    }
                    else if (BindableItems.Any(x => x.Status == LedState.Fail))
                    {
                        return LedState.Fail;
                    }
                    else if (BindableItems.All(x => x.Status == LedState.Success))
                    {
                        return LedState.Success;
                    }
                    else
                    {
                        return LedState.Idle;
                    }
                }
            }
        }

        [BsonIgnore]
        public bool? IsSelected
        {
            get
            {
                if (BindableItems.Count == 0 || !IsEnabled)
                {
                    return null;
                }
                else
                {
                    if (BindableItems.All(x => x.IsSelected))
                    {
                        return true;
                    }
                    else if (BindableItems.All(x => !x.IsSelected))
                    {
                        return false;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            set
            {
                if (value.HasValue && IsEnabled)
                {
                    foreach (var item in BindableItems)
                    {
                        item.IsSelected = value.Value;
                    }
                }
            }
        }

        [BsonIgnore]
        public DeviceProtocolItem ItemInProgress
        {
            get
            {
                return itemInProgress;
            }
            set
            {
                itemInProgress = value;
                OnPropertyChanged();
            }
        }

        public Guid[] StandardConfigurationIDs { get; set; }

        [BsonIgnore]
        [field: NonSerialized]
        public ProtocolStandard[] Standards { get; set; }

        [BsonIgnore]
        [field: NonSerialized]
        public BindableCollection<DeviceProtocolItem> BindableItems { get; }
            = new BindableCollection<DeviceProtocolItem>();

        public DeviceProtocolItem[] Items { get; set; }

        public void Initialize(DeviceProtocol owner, bool fromCliche)
        {
            Owner = owner;

            var modeInfo = ProtocolFunctions.GetPairedModeInfo(this);

            UpdateDevice();

            BindableItems.GetInstanceDelegate = () => ProtocolFunctions.GetPairedModeInfo(this).GetProtocolRow(this);
            BindableItems.GetCopyDelegate = (item) => ProtocolFunctions.GetPairedModeInfo(this).GetProtocolRowCopy(this, item, RowLoadMode.FromCopy);

            if (modeInfo.Standards != null)
            {
                ProtocolStandard[] standards = new ProtocolStandard[modeInfo.Standards.Length];

                for (int i = 0; i < standards.Length; i++)
                {
                    Guid index;

                    if (i < StandardConfigurationIDs?.Length)
                    {
                        index = StandardConfigurationIDs[i];
                    }
                    else
                    {
                        index = Guid.Empty;
                    }

                    standards[i] = new ProtocolStandard(this, index, modeInfo.Standards[i]);
                }

                Standards = standards;
            }
            else
            {
                Standards = new ProtocolStandard[0];
            }

            if (Items != null)
            {
                foreach (var item in Items)
                {
                    item.Values = item.StoredValues;

                    var newItem = ProtocolFunctions.GetPairedModeInfo(this).GetProtocolRowCopy(this, item, fromCliche ? RowLoadMode.FromCliche : RowLoadMode.FromProtocol);
                    BindableItems.Add(newItem);
                }
            }

            UpdateDisplayedName();
        }

        public void PrepareToStore(bool toCliche)
        {
            foreach (var item in BindableItems)
            {
                var items = item.Values
                    .Where(x => !(x is IReadOnlyValueInfo) || (x is IReadOnlyValueInfo readOnly && !readOnly.IsReadOnly));

                if (toCliche)
                {
                    items = items.Where(x => !(x is ValueInfo valueInfo && valueInfo.Type == ValueInfoType.Component && valueInfo.Function.Direction == Direction.Get));
                }

                item.StoredValues = items.Select(x => new BaseValueInfo(x)).ToArray();
            }

            Items = BindableItems.ToArray();

            StandardConfigurationIDs = Standards?.Select(x => x.ConfigurationID).ToArray();
        }

        public void UpdateDevice()
        {
            if (Owner != null)
            {
                IsEnabled = Owner.Device.Device.Functions.ContainsKey(ProtocolFunctions.GetPairedModeInfo(this).SourceMode);
            }
            else
            {
                IsEnabled = false;
            }

            if (Standards != null)
            {
                foreach (var standard in Standards)
                {
                    standard.UpdateDevice(false);
                }
            }

            UpdateItems();
        }

        public void UpdateItems()
        {
            if (Owner != null && Standards != null)
            {
                var currentItems = BindableItems.ToArray();
                var selected = BindableItems.SelectedItem;

                BindableItems.Clear();

                foreach (var current in currentItems)
                {
                    var newItem = ProtocolFunctions.GetPairedModeInfo(this).GetProtocolRowCopy(this, current, RowLoadMode.FromCopy);
                    BindableItems.Add(newItem);

                    if (current == selected)
                    {
                        BindableItems.SelectedItem = newItem;
                    }
                }
            }
        }

        public void UpdateDisplayedName()
        {
            if (IsEnabled)
            {
                string[] standards = Standards?.Select(x => x.Device?.Device.Configuration.Name).Where(x => x != null).ToArray();

                if (standards?.Length > 0)
                {
                    DisplayedName = $"{Name} ({string.Join(", ", standards)})";
                }
                else
                {
                    DisplayedName = Name;
                }
            }
            else
            {
                DisplayedName = $"{Name} (Режим недоступен)";
            }

            OnPropertyChanged(nameof(DisplayedName));
        }

        private async Task Remove()
        {
            var result = await Owner.Owner.Owner.ShowMessageAsync(
                "Удалить", $"Вы действительно хотите удалить блок \"{Name}\"?",
                MessageDialogStyle.AffirmativeAndNegative,
                new MetroDialogSettings
                {
                    AffirmativeButtonText = "Да",
                    NegativeButtonText = "Нет",
                    DefaultButtonFocus = MessageDialogResult.Affirmative
                });

            if (result == MessageDialogResult.Affirmative)
            {
                Owner.BindableBlocks.Remove(this);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
