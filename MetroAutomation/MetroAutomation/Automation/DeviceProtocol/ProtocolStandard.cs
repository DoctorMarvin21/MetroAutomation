using MetroAutomation.Calibration;
using MetroAutomation.Model;

namespace MetroAutomation.Automation
{
    public class ProtocolStandard
    {
        private int configurationID;

        public ProtocolStandard(DeviceProtocolBlock owner, int configurationID, StandardInfo info)
        {
            Owner = owner;
            this.configurationID = configurationID;
            Info = info;

            AllowedStandards = LiteDBAdaptor.GetPairedStandardNames(Info.Mode);

            UpdateDevice();
        }

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
                UpdateDevice();
                Owner.UpdateItems();
            }
        }

        public StandardInfo Info { get; set; }

        public NameID[] AllowedStandards { get; }

        public Device Device { get; set; }

        public Function Function { get; set; }

        private void UpdateDevice()
        {
            Device = Owner.Owner.Owner.ConnectionManager.LoadDevice(ConfigurationID).Device;

            if (Device.Functions.TryGetValue(Info.Mode, out Function function))
            {
                Function = function;
            }
            else
            {
                // Setting default function to avoid exceptions
                Function = Function.GetFunction(Device, Info.Mode);
            }
        }
    }
}
