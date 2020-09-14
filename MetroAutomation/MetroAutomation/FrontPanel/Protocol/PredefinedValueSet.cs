using MetroAutomation.Calibration;
using System.Collections.Generic;

namespace MetroAutomation.FrontPanel
{
    public class ValueSet
    {
        public int DeviceIndex { get; set; }

        public Dictionary<Mode, BaseValueInfo[][]> DeviceValues { get; set; }
    }
}
