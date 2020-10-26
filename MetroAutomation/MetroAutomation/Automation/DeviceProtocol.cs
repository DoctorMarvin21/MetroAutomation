using LiteDB;
using MetroAutomation.Calibration;
using MetroAutomation.Model;
using MetroAutomation.ViewModel;
using System;

namespace MetroAutomation.Automation
{
    [Serializable]
    public class DeviceProtocol : IDataObject
    {
        [NonSerialized]
        private MainViewModel owner;
        private int configurationID;

        [BsonIgnore]
        public MainViewModel Owner
        {
            get
            {
                return owner;
            }
            set
            {
                owner = value;
                Update();
            }
        }

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
                Update();
            }
        }

        [BsonIgnore]
        [field: NonSerialized]
        public Device Device { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        [BsonIgnore]
        [field: NonSerialized]
        public BindableCollection<DeviceProtocolBlock> Blocks { get; }

        public DeviceProtocolBlock[] StoredBlocks { get; }

        public void Update()
        {
            if (Owner != null)
            {
                Device = Owner.ConnectionManager.LoadDevice(Device.ConfigurationID).Device;

                foreach (var block in Blocks)
                {
                    block.Owner = this;
                    block.Update();
                }
            }
        }

        public void IsUsed(Device device)
        {
            // TODO: implement
        }
    }
}
