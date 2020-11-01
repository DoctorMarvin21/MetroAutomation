using MetroAutomation.Calibration;
using MetroAutomation.Connection;
using MetroAutomation.Model;

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

            UpdateDevice(false);
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
                UpdateDevice(true);
                Owner.UpdateItems();
            }
        }

        public StandardInfo Info { get; set; }

        public NameID[] AllowedStandards { get; }

        public DeviceConnection Device { get; set; }

        public Function Function { get; set; }

        public void UpdateDevice(bool unloadUnused)
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

            if (unloadUnused && Owner.Standards != null)
            {
                Owner.Owner.Owner.ConnectionManager.UnloadUnusedDisconnectedDevices();
            }

            Owner.UpdateDisplayedName();
        }
    }
}
