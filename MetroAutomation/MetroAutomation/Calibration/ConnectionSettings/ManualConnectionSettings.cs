using System;

namespace MetroAutomation.Calibration
{
    [Serializable]
    public class ManualConnectionSettings : AdvancedConnectionSettings
    {
        public override ConnectionType Type => ConnectionType.Manual;

        public override void FromConnectionString(string connectionString) { }

        public override string ToConnectionString() => string.Empty;
    }
}