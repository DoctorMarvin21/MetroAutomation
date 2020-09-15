using System;

namespace MetroAutomation.Calibration
{
    [Serializable]
    public class RangeInfo
    {
        public string Output { get; set; }

        public string Alias { get; set; }

        public BaseValueInfo Range { get; set; }

        public ValueRange[] ComponentsRanges { get; set; }
    }
}
