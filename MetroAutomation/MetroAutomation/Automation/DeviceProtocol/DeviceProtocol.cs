using LiteDB;
using MetroAutomation.Calibration;
using MetroAutomation.Connection;
using MetroAutomation.Model;
using MetroAutomation.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MetroAutomation.Automation
{
    [Serializable]
    public class DeviceProtocol : DeviceProtocolCliche
    {
        public DeviceProtocol()
        {
            BindableBlocks.CollectionChanged += (s, e) =>
            {
                if (e.NewItems?.Count > 0)
                {
                    foreach (DeviceProtocolBlock item in e.NewItems)
                    {
                        item.PropertyChanged += (sp, ep) =>
                        {
                            if (ep.PropertyName == nameof(DeviceProtocolBlock.IsSelected)
                                || ep.PropertyName == nameof(DeviceProtocolBlock.IsEnabled))
                            {
                                OnPropertyChanged(nameof(IsSelected));
                            }
                        };
                    }
                }

                OnPropertyChanged(nameof(IsSelected));
            };
        }

        public string SerialNumber { get; set; }

        public string DeviceOwner { get; set; }

        [BsonIgnore]
        [field: NonSerialized]
        public DeviceConnection Device { get; private set; }

        [BsonIgnore]
        [field: NonSerialized]
        public MainViewModel Owner { get; private set; }

        [BsonIgnore]
        public bool? IsSelected
        {
            get
            {
                if (BindableBlocks.Count(x => x.IsEnabled && x.IsSelected.HasValue) == 0)
                {
                    return null;
                }
                else
                {
                    if (BindableBlocks.Where(x => x.IsEnabled && x.IsSelected.HasValue).All(x => x.IsSelected == true))
                    {
                        return true;
                    }
                    else if (BindableBlocks.Where(x => x.IsEnabled && x.IsSelected.HasValue).All(x => x.IsSelected == false))
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
                    foreach (var item in BindableBlocks)
                    {
                        item.IsSelected = value.Value;
                    }
                }
            }
        }

        [BsonIgnore]
        [field: NonSerialized]
        public NameID[] AllDevices { get; private set; }

        [BsonIgnore]
        [field: NonSerialized]
        public PairedModeInfo[] AllowedModes { get; private set; }

        [BsonIgnore]
        [field: NonSerialized]
        public PairedModeInfo SelectedMode { get; set; }

        [BsonIgnore]
        [field: NonSerialized]
        public AutomationProcessor Automation { get; private set; }

        [BsonIgnore]
        [field: NonSerialized]
        public BindableCollection<DeviceProtocolBlock> BindableBlocks { get; }
            = new BindableCollection<DeviceProtocolBlock>();

        public DeviceProtocolBlock[] Blocks { get; set; }

        public void Initialize(MainViewModel owner)
        {
            Owner = owner;
            Automation = new AutomationProcessor(this);

            AllDevices = LiteDBAdaptor.GetNames<DeviceConfiguration>();

            UpdateDevice();

            AllowedModes = ProtocolFunctions.GetModeInfo(Device.Device);

            BindableBlocks.GetInstanceDelegate = () =>
            {
                if (SelectedMode != null)
                {
                    var newBlock = new DeviceProtocolBlock
                    {
                        AutomationMode = SelectedMode.AutomationMode,
                        Name = SelectedMode.Name
                    };

                    newBlock.Initialize(this);

                    return newBlock;
                }
                else
                {
                    return null;
                }
            };

            if (AllowedModes.Length > 0)
            {
                SelectedMode = AllowedModes[0];
            }

            if (Blocks != null)
            {
                foreach (var block in Blocks)
                {
                    BindableBlocks.Add(block);
                    block.Initialize(this);
                }
            }
        }

        public void PrepareToStore()
        {
            foreach (var block in BindableBlocks)
            {
                block.PrepareToStore();
            }

            Blocks = BindableBlocks.ToArray();
        }

        public async Task RefreshDevices()
        {
            await Owner.DisconnectUnusedDevices();

            var usedDevices = GetUsedConnections();

            foreach (var connection in usedDevices)
            {
                await connection.Disconnect();
            }

            UpdateDevice();
        }

        protected override void OnConfigurationIDChanged()
        {
            UpdateDevice();
        }

        public void UpdateDevice()
        {
            if (Owner != null)
            {
                Device = Owner.ConnectionManager.LoadDevice(ConfigurationID);

                foreach (var block in BindableBlocks)
                {
                    block.UpdateDevice();
                }

                Owner.UnloadUnusedDevices();
            }
        }

        public DeviceConnection[] GetUsedConnections()
        {
            HashSet<DeviceConnection> usedConnections = new HashSet<DeviceConnection>
            {
                Device
            };

            foreach (var block in BindableBlocks)
            {
                if (block.Standards != null)
                {
                    foreach (var standard in block.Standards)
                    {
                        if (standard != null)
                        {
                            usedConnections.Add(standard.Device);
                        }
                    }
                }
            }

            return usedConnections.ToArray();
        }
    }
}
