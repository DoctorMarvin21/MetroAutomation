using System.Collections.Generic;

namespace MetroAutomation.Calibration
{
    public static class ConnectionUtils
    {
        public const string InstrumentTag = "INSTR";

        public const string Splitter = "::";

        public static Dictionary<ConnectionType, string> Tags { get; }
            = new Dictionary<ConnectionType, string>()
        {
            { ConnectionType.Manual, null },
            { ConnectionType.Serial, "ASRL" },
            { ConnectionType.Gpib, "GPIB" },
            { ConnectionType.Usb, "USB" },
        };

        public static AdvancedConnectionSettings GetConnectionSettingsByType(ConnectionType connectionType)
        {
            switch (connectionType)
            {
                case ConnectionType.Manual:
                    {
                        return new ManualConnectionSettings();
                    }
                case ConnectionType.Serial:
                    {
                        return new SerialConnectionSettings();
                    }
                case ConnectionType.Gpib:
                    {
                        return new GpibConnectionSettings();
                    }
                case ConnectionType.Usb:
                    {
                        return new UsbConnectionSettings();
                    }
                default:
                    {
                        return null;
                    }
            }
        }

        public static AdvancedConnectionSettings GetConnectionSettingsByResourceName(string resourceName)
        {
            if (resourceName == null)
            {
                return new ManualConnectionSettings();
            }

            AdvancedConnectionSettings connectionSettings;

            if (resourceName.StartsWith(Tags[ConnectionType.Serial]))
            {
                connectionSettings = new SerialConnectionSettings();
            }
            else if (resourceName.StartsWith(Tags[ConnectionType.Gpib]))
            {
                connectionSettings = new GpibConnectionSettings();
            }
            else if (resourceName.StartsWith(Tags[ConnectionType.Usb]))
            {
                connectionSettings = new UsbConnectionSettings();
            }
            else
            {
                connectionSettings = new ManualConnectionSettings();
            }

            connectionSettings.FromConnectionString(resourceName);

            return connectionSettings;
        }
    }
}
