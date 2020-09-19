using MetroAutomation.Calibration;

namespace MetroAutomation.FrontPanel
{
    public class Fluke5520FrontPanelViewModel : FrontPanelViewModel
    {
        public Fluke5520FrontPanelViewModel(Device device)
            : base(device)
        {
            if (Device.Functions.TryGetValue(Mode.SetACI, out var aci))
            {
                aci.AttachedCommands.Add(new LCompCommand(aci));
            }

            if (Device.Functions.TryGetValue(Mode.SetACP, out var acp))
            {
                acp.AttachedCommands.Add(new LCompCommand(acp));
            }

            if (Device.Functions.TryGetValue(Mode.SetRES2W, out var res2w))
            {
                res2w.AttachedCommands.Add(new ZCompCommand(res2w));
            }

            if (Device.Functions.TryGetValue(Mode.SetCAP2W, out var cap2w))
            {
                cap2w.AttachedCommands.Add(new ZCompCommand(cap2w));
            }
        }

        public override FrontPanelType Type => FrontPanelType.Fluke5520;
    }
}
