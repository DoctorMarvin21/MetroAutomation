using LiteDB;
using MetroAutomation.Calibration;
using MetroAutomation.Connection;
using MetroAutomation.Model;
using MetroAutomation.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MetroAutomation.Automation
{
    [Serializable]
    public class DeviceProtocol : IDataObject
    {
        private int configurationID;

        [BsonIgnore]
        [field: NonSerialized]
        public MainViewModel Owner { get; private set; }

        public int ID { get; set; }

        public int ConfigurationID
        {
            get
            {
                return configurationID;
            }
            set
            {
                configurationID = value;
                UpdateDevice();
            }
        }

        [BsonIgnore]
        [field: NonSerialized]
        public NameID[] AllDevices { get; private set; }

        [BsonIgnore]
        [field: NonSerialized]
        public DeviceConnection Device { get; private set; }

        [BsonIgnore]
        [field: NonSerialized]
        public PairedModeInfo[] AllowedModes { get; private set; }

        [BsonIgnore]
        [field: NonSerialized]
        public PairedModeInfo SelectedMode { get; set; }

        [BsonIgnore]
        [field: NonSerialized]
        public AutomationProcessor Automation { get; private set; }

        public string Name { get; set; }

        public string Type { get; set; }

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
                foreach (var standard in block.Standards)
                {
                    if (standard != null)
                    {
                        usedConnections.Add(standard.Device);
                    }
                }
            }

            return usedConnections.ToArray();
        }
    }
}
