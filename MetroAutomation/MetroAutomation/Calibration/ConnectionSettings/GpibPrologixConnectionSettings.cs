using System;

namespace MetroAutomation.Calibration
{
    [Serializable]
    public class GpibPrologixConnectionSettings : GpibBaseConnectionSettings
    {
        public override ConnectionType Type => ConnectionType.GpibPrologix;

        public override string ToConnectionString()
        {
            return $"{ConnectionUtils.Tags[ConnectionType.Serial]}{BoardIndex}{ConnectionUtils.Splitter}{ConnectionUtils.InstrumentTag}";
        }
    }
}