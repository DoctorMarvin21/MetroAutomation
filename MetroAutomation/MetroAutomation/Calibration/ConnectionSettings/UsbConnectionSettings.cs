using System;

namespace MetroAutomation.Calibration
{
    [Serializable]
    public class UsbConnectionSettings : AdvancedConnectionSettings
    {
        public override ConnectionType Type => ConnectionType.Usb;

        public string ManufacturerID { get; set; }

        public string ModelCode { get; set; }

        public string SerialNumber { get; set; }

        public int? UsbInterfaceNumber { get; set; }

        public override void FromConnectionString(string connectionString)
        {
            string[] split = connectionString.Split(ConnectionUtils.Splitter);

            if (split.Length > 0 && int.TryParse(split[0].Replace(ConnectionUtils.Tags[Type], string.Empty), out int boardIndex))
            {
                BoardIndex = boardIndex;
            }
            else
            {
                BoardIndex = null;
            }

            if (split.Length > 1)
            {
                ManufacturerID = split[1];
            }
            else
            {
                ManufacturerID = null;
            }

            if (split.Length > 2)
            {
                ModelCode = split[2];
            }
            else
            {
                ModelCode = null;
            }

            if (split.Length > 3)
            {
                SerialNumber = split[3];
            }
            else
            {
                SerialNumber = null;
            }

            if (split.Length > 4 && int.TryParse(split[4], out int interfaceNumber))
            {
                UsbInterfaceNumber = interfaceNumber;
            }
            else
            {
                UsbInterfaceNumber = null;
            }
        }

        public override string ToConnectionString()
        {
            string connectionString = $"{ConnectionUtils.Tags[Type]}{BoardIndex}" +
                $"{ConnectionUtils.Splitter}{ManufacturerID}{ConnectionUtils.Splitter}{ModelCode}";

            if (!string.IsNullOrEmpty(SerialNumber))
            {
                connectionString += $"{ConnectionUtils.Splitter}{SerialNumber}";
            }

            if (UsbInterfaceNumber != null)
            {
                connectionString += $"{ConnectionUtils.Splitter}{UsbInterfaceNumber}";
            }

            connectionString += $"{ConnectionUtils.Splitter}{ConnectionUtils.InstrumentTag}";

            return connectionString;
        }
    }
}