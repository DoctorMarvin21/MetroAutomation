using LiteDB;
using MetroAutomation.Calibration;
using MetroAutomation.Connection;
using MetroAutomation.Model;
using MetroAutomation.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace MetroAutomation.Automation
{
    public enum WorkStatus
    {
        [Description("В работе")]
        InWork,
        [Description("Годен")]
        Success,
        [Description("Не годен")]
        Fail
    }

    [Serializable]
    public class DeviceProtocol : DeviceProtocolCliche, IDeviceProtocolDisplayed
    {
        [NonSerialized]
        private PairedModeInfo[] allowedModes;

        [NonSerialized]
        private PairedModeInfo selectedMode;

        [NonSerialized]
        private DeviceConnection device;

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

        public string ProtocolNumber { get; set; }

        public string SerialNumber { get; set; }

        public string DeviceOwner { get; set; }

        public DateTime CalibrationDate { get; set; } = DateTime.Now;

        public WorkStatus WorkStatus { get; set; }

        [BsonIgnore]
        [field: NonSerialized]
        public WorkStatus[] AvailableWorkStatuses { get; }
            = new[] { WorkStatus.InWork, WorkStatus.Success, WorkStatus.Fail };

        [BsonIgnore]
        public DeviceConnection Device
        {
            get
            {
                return device;
            }
            private set
            {
                device = value;
                OnPropertyChanged();
            }
        }

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
        public PairedModeInfo[] AllowedModes
        {
            get
            {
                return allowedModes;
            }
            private set
            {
                allowedModes = value;
                OnPropertyChanged();
            }
        }

        [BsonIgnore]
        public PairedModeInfo SelectedMode
        {
            get
            {
                return selectedMode;
            }
            set
            {
                selectedMode = value;
                OnPropertyChanged();
            }
        }

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

            UpdateDevice(false);

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

            if (Blocks != null)
            {
                foreach (var block in Blocks)
                {
                    BindableBlocks.Add(block);
                    block.Initialize(this);
                }
            }

            OnPropertyChanged(nameof(IsSelected));
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
            await Owner.ConnectionManager.DisconnectAndUnloadUnusedDevices();

            var usedDevices = GetUsedConnections();

            foreach (var connection in usedDevices)
            {
                await connection.Disconnect();
            }

            UpdateDevice(false);
        }

        protected override void OnConfigurationIDChanged()
        {
            UpdateDevice(true);
        }

        public void UpdateDevice(bool unloadUnused)
        {
            if (Owner != null)
            {
                var blocks = BindableBlocks.ToArray();
                BindableBlocks.Clear();

                Device = Owner.ConnectionManager.LoadDevice(ConfigurationID);

                AllowedModes = ProtocolFunctions.GetModeInfo(Device.Device);

                if (AllowedModes.Length > 0)
                {
                    SelectedMode = AllowedModes[0];
                }

                foreach (var block in blocks)
                {
                    block.UpdateDevice();
                }

                foreach (var block in blocks)
                {
                    BindableBlocks.Add(block);
                }

                if (unloadUnused)
                {
                    Owner.ConnectionManager.UnloadUnusedDisconnectedDevices();
                }
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
