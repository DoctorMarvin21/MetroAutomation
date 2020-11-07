using MetroAutomation.Calibration;

namespace MetroAutomation.FrontPanel
{
    public enum Fluke9100UutType
    {
        RES,
        CAP
    }

    public class Fluke9100FrontPanelViewModel : FrontPanelViewModel
    {
        public Fluke9100FrontPanelViewModel(Device device)
            : base(device)
        {
            if (device.Functions.TryGetValue(Mode.SetRES2W, out var res2w))
            {
                res2w.AttachedCommands.Add(new Fluke9100UutAttachedCommand(Fluke9100UutType.RES, res2w));
            }

            if (device.Functions.TryGetValue(Mode.SetRES4W, out var res4w))
            {
                res4w.AttachedCommands.Add(new Fluke9100UutAttachedCommand(Fluke9100UutType.RES, res4w));
            }

            if (device.Functions.TryGetValue(Mode.SetCAP2W, out var cap2w))
            {
                cap2w.AttachedCommands.Add(new Fluke9100UutAttachedCommand(Fluke9100UutType.CAP, cap2w));
            }

            if (device.Functions.TryGetValue(Mode.SetCAP4W, out var cap4w))
            {
                cap4w.AttachedCommands.Add(new Fluke9100UutAttachedCommand(Fluke9100UutType.CAP, cap4w));
            }
        }

        public override FrontPanelType Type => FrontPanelType.Fluke9100;
    }
}
