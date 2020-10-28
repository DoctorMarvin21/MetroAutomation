using LiteDB;
using MetroAutomation.Calibration;
using MetroAutomation.ViewModel;
using System;

namespace MetroAutomation.Automation
{
    [Serializable]
    public class DeviceProtocolBlock
    {
        [BsonIgnore]
        [field: NonSerialized]
        public DeviceProtocol Owner { get; set; }

        public string Name { get; set; }

        public Mode DeviceMode { get; set; }

        [BsonIgnore]
        [field: NonSerialized]
        public Function DeviceFunction { get; set; }

        public StandardAndMode[] Standards { get; set; }

        [BsonIgnore]
        [field: NonSerialized]
        public BindableCollection<DeviceProtocolItem> ProtocolItems { get; }

        public void Update()
        {
            if (Owner != null)
            {
                if (Owner.Device.Functions.TryGetValue(DeviceMode, out Function function))
                {
                    DeviceFunction = function;
                }
                else
                {
                    // Setting default function to avoid exceptions
                    DeviceFunction = Function.GetFunction(Owner.Device, DeviceMode);
                }

                if (Standards != null)
                {
                    foreach (var standard in Standards)
                    {
                        standard.Owner = this;
                        standard.Update();
                    }
                }

                if (ProtocolItems?.Count > 0)
                {
                    foreach (var item in ProtocolItems)
                    {
                        item.Owner = this;
                        item.Update();
                    }
                }
            }
        }
    }
}
