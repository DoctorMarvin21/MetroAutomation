using MetroAutomation.Calibration;
using System.Threading.Tasks;

namespace MetroAutomation.FrontPanel
{
    public class LCompCommand : AttachedCommand
    {
        public LCompCommand(Device device)
            : base(device)
        {
            AutoExecute = true;
        }

        private bool lComp;

        public bool LComp
        {
            get
            {
                return lComp;
            }
            set
            {
                lComp = value;
                OnPropertyChanged();
            }
        }

        public override async Task Process()
        {
            string command = $"LCOMP {(LComp ? "ON" : "OFF")};*OPC?";

            if (!await Device.QueryAction(command, false))
            {
                LComp = false;
            }
        }
    }


    public class Fluke5520FrontPanelViewModel : FrontPanelViewModel
    {
        public Fluke5520FrontPanelViewModel(Device device)
            : base(device)
        {
            LComp = new LCompCommand(device);
            Device.Functions[Mode.SetACI].AttachedCommands.Add(LComp);
        }

        public override FrontPanelType Type => FrontPanelType.Fluke5520;

        public LCompCommand LComp { get; }
    }
}
