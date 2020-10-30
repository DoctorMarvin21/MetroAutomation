using LiteDB;
using MetroAutomation.Calibration;
using MetroAutomation.Controls;
using MetroAutomation.ViewModel;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MetroAutomation.Automation
{
    [Serializable]
    public class DeviceProtocolBlock : INotifyPropertyChanged
    {
        [NonSerialized]
        private bool isEnabled;
        private string name;

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

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
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

        public ICommand RemoveFromOwner => new CommandHandler((arg) => Owner?.BindableBlocks.Remove(this));

        [BsonIgnore]
        [field: NonSerialized]
        public Function DeviceFunction { get; private set; }

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
            }
        }

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
                    else if (BindableItems.Any(x => x.Status == LedState.Success))
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
                if (BindableItems.Count == 0)
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
                if (value.HasValue)
                {
                    foreach (var item in BindableItems)
                    {
                        item.IsSelected = value.Value;
                    }
                }
            }
        }

        public int[] StandardConfigurationIDs { get; set; }

        [BsonIgnore]
        [field: NonSerialized]
        public ProtocolStandard[] Standards { get; set; }

        [BsonIgnore]
        [field: NonSerialized]
        public BindableCollection<DeviceProtocolItem> BindableItems { get; } = new BindableCollection<DeviceProtocolItem>();

        public DeviceProtocolItem[] Items { get; set; }

        public void Initialize(DeviceProtocol owner)
        {
            Owner = owner;

            var modeInfo = ProtocolFunctions.GetPairedModeInfo(this);

            UpdateDevice();

            BindableItems.GetInstanceDelegate = () => ProtocolFunctions.GetPairedModeInfo(this).GetProtocolRow(this);
            BindableItems.GetCopyDelegate = (item) => ProtocolFunctions.GetPairedModeInfo(this).GetProtocolRowCopy(this, item, false);

            if (modeInfo.Standards != null)
            {
                ProtocolStandard[] standards = new ProtocolStandard[modeInfo.Standards.Length];

                for (int i = 0; i < standards.Length; i++)
                {
                    int index;

                    if (i < StandardConfigurationIDs?.Length)
                    {
                        index = StandardConfigurationIDs[i];
                    }
                    else
                    {
                        index = 0;
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

                    var newItem = ProtocolFunctions.GetPairedModeInfo(this).GetProtocolRowCopy(this, item, true);
                    BindableItems.Add(newItem);
                }
            }

            UpdateDisplayedName();
        }

        public void PrepareToStore()
        {
            foreach (var item in BindableItems)
            {
                item.StoredValues = item.Values
                    .Where(x => !(x is IReadOnlyValueInfo) || (x is IReadOnlyValueInfo readOnly && !readOnly.IsReadOnly))
                    .Select(x => new BaseValueInfo(x)).ToArray();
            }

            Items = BindableItems.ToArray();

            StandardConfigurationIDs = Standards?.Select(x => x.ConfigurationID).ToArray();
        }

        public void UpdateDevice()
        {
            if (Owner != null)
            {
                var modeInfo = ProtocolFunctions.GetPairedModeInfo(this);

                if (Owner.Device.Device.Functions.TryGetValue(modeInfo.SourceMode, out Function function))
                {
                    DeviceFunction = function;
                    IsEnabled = true;
                }
                else
                {
                    // Setting default function to avoid exceptions
                    DeviceFunction = Function.GetFunction(Owner.Device.Device, modeInfo.SourceMode);
                    IsEnabled = false;
                }
            }
            else
            {
                IsEnabled = false;
            }

            UpdateItems();
        }

        public void UpdateItems()
        {
            if (Owner != null)
            {
                var currentItems = BindableItems.ToArray();
                var selected = BindableItems.SelectedItem;

                BindableItems.Clear();

                foreach (var current in currentItems)
                {
                    var newItem = ProtocolFunctions.GetPairedModeInfo(this).GetProtocolRowCopy(this, current, false);
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
            string[] standards = Standards?.Select(x => x.Device?.Device.Configuration.Name).Where(x => x != null).ToArray();

            if (standards?.Length > 0)
            {
                DisplayedName = $"{Name} ({string.Join(", ", standards)})";
            }
            else
            {
                DisplayedName = Name;
            }

            OnPropertyChanged(nameof(DisplayedName));
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
