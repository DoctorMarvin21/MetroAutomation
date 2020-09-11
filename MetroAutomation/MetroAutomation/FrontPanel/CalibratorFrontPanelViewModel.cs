using MetroAutomation.Calibration;

namespace MetroAutomation.FrontPanel
{
    public class CalibratorFrontPanelViewModel : FrontPanelViewModel
    {
        public CalibratorFrontPanelViewModel(Device device)
            : base(device)
        {
        }

        public override FrontPanelType Type => FrontPanelType.Calibrator;
    }
}
