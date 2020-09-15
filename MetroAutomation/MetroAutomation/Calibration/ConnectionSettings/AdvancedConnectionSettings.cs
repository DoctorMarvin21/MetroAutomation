using System;
namespace MetroAutomation.Calibration
{
    [Serializable]
    public abstract class AdvancedConnectionSettings
    {
        public int? BoardIndex { get; set; }

        public abstract ConnectionType Type { get; }

        public abstract void FromConnectionString(string connectionString);

        public abstract string ToConnectionString();
    }
}