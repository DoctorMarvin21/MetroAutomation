using System;

namespace MetroAutomation.Calibration
{
    [Serializable]
    public class ValueMultiplier
    {
        public ValueMultiplier()
        {
        }

        public ValueMultiplier(string name, BaseValueInfo value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }

        public BaseValueInfo Value { get; set; }
    }
}
