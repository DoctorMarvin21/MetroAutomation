using LiteDB;
using MetroAutomation.Calibration;
using MetroAutomation.ViewModel;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace MetroAutomation.Automation
{
    [Serializable]
    public class DeviceProtocolBlock : INotifyPropertyChanged
    {
        [NonSerialized]
        private bool isEnabled;

        [BsonIgnore]
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        [BsonIgnore]
        [field: NonSerialized]
        public DeviceProtocol Owner { get; private set; }

        public string Name { get; set; }

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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnabled)));
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
            BindableItems.GetCopyDelegate = (item) => ProtocolFunctions.GetPairedModeInfo(this).GetProtocolRowCopy(this, item);

            if (modeInfo.Standards != null)
            {
                Standards = new ProtocolStandard[modeInfo.Standards.Length];

                for (int i = 0; i < Standards.Length; i++)
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

                    Standards[i] = new ProtocolStandard(this, index, modeInfo.Standards[i]);
                }
            }
            else
            {
                Standards = new ProtocolStandard[0];
            }

            if (Items != null)
            {
                foreach (var item in Items)
                {
                    BindableItems.Add(item);
                    item.Values = item.StoredValues;
                }

                UpdateItems();
            }
        }

        public void PrepareToStore()
        {
            foreach (var item in BindableItems)
            {
                item.StoredValues = item.Values.Select(x => new BaseValueInfo(x)).ToArray();
            }

            Items = BindableItems.ToArray();

            StandardConfigurationIDs = Standards?.Select(x => x.ConfigurationID).ToArray();
        }

        public void UpdateDevice()
        {
            if (Owner != null)
            {
                var modeInfo = ProtocolFunctions.GetPairedModeInfo(this);

                if (Owner.Device.Functions.TryGetValue(modeInfo.SourceMode, out Function function))
                {
                    DeviceFunction = function;
                    IsEnabled = true;
                }
                else
                {
                    // Setting default function to avoid exceptions
                    DeviceFunction = Function.GetFunction(Owner.Device, modeInfo.SourceMode);
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
                    var newItem = ProtocolFunctions.GetPairedModeInfo(this).GetProtocolRowCopy(this, current);
                    BindableItems.Add(newItem);

                    if (current == selected)
                    {
                        BindableItems.SelectedItem = newItem;
                    }
                }
            }
        }
    }
}
