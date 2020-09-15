using System;

namespace MetroAutomation.Calibration
{
    [Serializable]
    public class FunctionCommandSet
    {
        public FunctionCommandSet()
        {
        }

        public FunctionCommandSet(Mode mode)
        {
            Mode = mode;
        }

        public Mode Mode { get; set; }

        public string FunctionCommand { get; set; }

        public string RangeCommand { get; set; }

        public string ValueCommand { get; set; }
    }
}
