using MetroAutomation.Calibration;
using MetroAutomation.Connection;
using MetroAutomation.Model;
using System;

namespace MetroAutomation.Automation
{
    public class ProtocolStandard
    {
        private Guid configurationID;

        public ProtocolStandard(DeviceProtocolBlock owner, Guid configurationID, StandardInfo info)
        {
            Owner = owner;
            Info = info;

            AllowedStandards = LiteDBAdaptor.GetPairedStandardNames(Info.Mode);

            if (configurationID == Guid.Empty && AllowedStandards.Length > 0)
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

        public Guid ConfigurationID
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

        public void UpdateDevice(bool unloadUnused)
        {
            Device = Owner.Owner.Owner.ConnectionManager.LoadDevice(ConfigurationID);

            if (unloadUnused && Owner.Standards != null)
            {
                Owner.Owner.Owner.ConnectionManager.UnloadUnusedDisconnectedDevices();
            }

            Owner.UpdateDisplayedName();
        }
    }
}
