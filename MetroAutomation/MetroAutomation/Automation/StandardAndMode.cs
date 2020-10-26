using LiteDB;
using MetroAutomation.Calibration;
using System;

namespace MetroAutomation.Automation
{
    [Serializable]
    public class StandardAndMode
    {
        private int configurationID;

        [BsonIgnore]
        [field: NonSerialized]
        public DeviceProtocolBlock Owner { get; set; }

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
        public Device Standard { get; set; }

        public Mode Mode { get; set; }

        [BsonIgnore]
        [field: NonSerialized]
        public Function Function { get; set; }

        public void Update()
        {
            if (Owner != null)
            {
                Standard = Owner.Owner.Owner.ConnectionManager.LoadDevice(ConfigurationID).Device;

                if (Standard.Functions.TryGetValue(Mode, out Function function))
                {
                    Function = function;
                }
                else
                {
                    // Setting default function to avoid exceptions
                    Function = Function.GetFunction(Standard, Mode);
                }
            }
        }
    }
}
