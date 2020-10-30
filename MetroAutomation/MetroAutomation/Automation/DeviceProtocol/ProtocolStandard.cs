using MetroAutomation.Calibration;
using MetroAutomation.Connection;
using MetroAutomation.Model;
using System.Linq;

namespace MetroAutomation.Automation
{
    public class ProtocolStandard
    {
        private int configurationID;

        public ProtocolStandard(DeviceProtocolBlock owner, int configurationID, StandardInfo info)
        {
            Owner = owner;
            Info = info;

            AllowedStandards = LiteDBAdaptor.GetPairedStandardNames(Info.Mode);

            if (configurationID == 0 && AllowedStandards.Length > 0)
            {
                this.configurationID = AllowedStandards[0].ID;
            }
            else
            {
                this.configurationID = configurationID;
            }

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

        public DeviceConnection Device { get; set; }

        public Function Function { get; set; }

        private void UpdateDevice()
        {
            Device = Owner.Owner.Owner.ConnectionManager.LoadDevice(ConfigurationID);

            if (Device.Device.Functions.TryGetValue(Info.Mode, out Function function))
            {
                Function = function;
            }
            else
            {
                // Setting default function to avoid exceptions
                Function = Function.GetFunction(Device.Device, Info.Mode);
            }

            if (Owner.Standards?.Contains(this) == true)
            {
                Owner.Owner.Owner.UnloadUnusedDevices();
            }

            Owner.UpdateDisplayedName();
        }
    }
}
