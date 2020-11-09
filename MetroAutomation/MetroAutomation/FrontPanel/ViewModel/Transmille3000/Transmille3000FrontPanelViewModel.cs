using MetroAutomation.Calibration;

namespace MetroAutomation.FrontPanel
{
    public class Transmille3000FrontPanelViewModel : FrontPanelViewModel
    {
        public Transmille3000FrontPanelViewModel(Device device)
            : base(device)
        {
            if (device.Functions.TryGetValue(Mode.SetRES2W, out var res2w))
            {
                res2w.AttachedCommands.Add(new TransmilleSimulatedModeCommand(res2w));
            }
        }

        public override FrontPanelType Type => FrontPanelType.Transmille3000;
    }
}
