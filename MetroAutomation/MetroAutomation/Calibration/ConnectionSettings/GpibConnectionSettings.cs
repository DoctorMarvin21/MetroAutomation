using System;

namespace MetroAutomation.Calibration
{
    [Serializable]
    public abstract class GpibBaseConnectionSettings : AdvancedConnectionSettings
    {
        public int PrimaryAddress { get; set; }

        public int? SecondaryAddress { get; set; }

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

            if (split.Length > 1 && int.TryParse(split[1], out int primaryAddress))
            {
                PrimaryAddress = primaryAddress;
            }
            else
            {
                PrimaryAddress = 0;
            }

            if (split.Length > 2 && int.TryParse(split[2], out int secondaryAddress))
            {
                SecondaryAddress = secondaryAddress;
            }
            else
            {
                SecondaryAddress = null;
            }
        }
    }

    [Serializable]
    public class GpibConnectionSettings : GpibBaseConnectionSettings
    {
        public override ConnectionType Type => ConnectionType.Gpib;

        public override string ToConnectionString()
        {
            string connectionString = $"{ConnectionUtils.Tags[Type]}{BoardIndex}{ConnectionUtils.Splitter}{PrimaryAddress}";

            if (SecondaryAddress != null)
            {
                connectionString += $"{ConnectionUtils.Splitter}{SecondaryAddress}";
            }

            connectionString += $"{ConnectionUtils.Splitter}{ConnectionUtils.InstrumentTag}";

            return connectionString;
        }
    }
}