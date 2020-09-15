using System;

namespace MetroAutomation.Calibration
{
    [Serializable]
    public class ValueMultiplier
    {
        public ValueMultiplier()
        {
        }

        public ValueMultiplier(string name, decimal multiplier)
        {
            Name = name;
            Multiplier = multiplier;
        }

        public string Name { get; set; }

        public decimal Multiplier { get; set; }
    }
}
