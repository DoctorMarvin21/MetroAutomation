using System;

namespace MetroAutomation.Calibration
{
    [Serializable]
    public class ActualValueInfo
    {
        public ActualValueInfo()
        {
        }

        public ActualValueInfo(IValueInfo valueInfo)
        {
            Value = new BaseValueInfo(valueInfo);
            ActualValue = new BaseValueInfo(valueInfo);
        }

        public BaseValueInfo Value { get; set; }

        public BaseValueInfo ActualValue { get; set; }
    }
}
