﻿using MetroAutomation.Calibration;
using MetroAutomation.Model;

namespace MetroAutomation.FrontPanel
{
    public class FrontPanelValueSet : IDataObject
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public DeviceValueSet[] Values { get; set; }
    }

    public class DeviceValueSet
    {
        public int ConfigurationID { get; set; }

        public FunctionValueSet[] Values { get; set; }
    }

    public class FunctionValueSet
    {
        public Mode Mode { get; set; }

        public ValueSet[] Values { get; set; }
    }

    public class ValueSet
    {
        public decimal? Multiplier { get; set; }

        public BaseValueInfo[] Values { get; set; }
    }
}
