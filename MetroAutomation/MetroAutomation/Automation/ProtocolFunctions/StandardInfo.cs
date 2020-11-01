using MetroAutomation.Calibration;

namespace MetroAutomation.Automation
{
    public class StandardInfo
    {
        public StandardInfo(string description, Mode mode)
        {
            Description = description;
            Mode = mode;
        }

        public string Description { get; set; }

        public Mode Mode { get; set; }
    }
}
