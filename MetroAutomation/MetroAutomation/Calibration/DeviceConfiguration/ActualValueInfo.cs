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

        public override bool Equals(object obj)
        {
            if (obj is ActualValueInfo valueInfo)
            {
                if (Value == null && valueInfo.Value == null)
                {
                    return true;
                }
                else
                {
                    return valueInfo.Value?.Equals(this?.Value) ?? false;
                }
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }
    }
}
