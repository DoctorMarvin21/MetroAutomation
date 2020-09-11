using MetroAutomation.Calibration;

namespace MetroAutomation.FrontPanel
{
    public class BaseFrontPanelViewModel : FrontPanelViewModel
    {
        public BaseFrontPanelViewModel(Device device)
            : base(device)
        {
        }

        public override FrontPanelType Type => FrontPanelType.Base;
    }
}
