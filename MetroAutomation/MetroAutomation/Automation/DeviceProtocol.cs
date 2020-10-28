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
        private int configurationID;

        [BsonIgnore]
        [field: NonSerialized]
        public MainViewModel Owner { get; set; }

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
        public BindableCollection<DeviceProtocolBlock> BindableBlocks { get; } = new BindableCollection<DeviceProtocolBlock>();

        public DeviceProtocolBlock[] Blocks { get; set; }

        public void Update()
        {
            if (Owner != null)
            {
                Device = Owner.ConnectionManager.LoadDevice(ConfigurationID).Device;

                foreach (var block in BindableBlocks)
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
