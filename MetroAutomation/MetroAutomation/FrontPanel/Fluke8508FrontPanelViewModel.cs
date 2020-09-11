using MetroAutomation.Calibration;

namespace MetroAutomation.FrontPanel
{
    public class Fluke8508FrontPanelViewModel : FrontPanelViewModel
    {
        public Fluke8508FrontPanelViewModel(Device device)
            : base(device)
        {
        }

        public override FrontPanelType Type => FrontPanelType.Fluke8508;
    }
}
