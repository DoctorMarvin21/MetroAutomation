using MetroAutomation.Calibration;
using MetroAutomation.Model;
using System;

namespace MetroAutomation.FrontPanel
{
    [Serializable]
    public class FrontPanelValueSet : IDataObject
    {
        public Guid ID { get; set; }

        public string Name { get; set; }

        public DeviceValueSet[] Values { get; set; }
    }

    [Serializable]
    public class DeviceValueSet
    {
        public Guid ConfigurationID { get; set; }

        public FunctionValueSet[] Values { get; set; }
    }

    [Serializable]
    public class FunctionValueSet
    {
        public Mode Mode { get; set; }

        public ValueSet[] Values { get; set; }
    }

    [Serializable]
    public class ValueSet
    {
        public decimal? Multiplier { get; set; }

        public BaseValueInfo[] Values { get; set; }
    }
}
