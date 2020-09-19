using MetroAutomation.Calibration;
using System.Threading.Tasks;

namespace MetroAutomation.FrontPanel
{
    public class LCompCommand : AttachedCommand
    {

        private bool lComp;

        public LCompCommand(Device device)
            : base(device)
        {
        }

        public override AutoExecuteType AutoExecute => AutoExecuteType.AfterValue;

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
            else
            {
                LComp = await Device.QueryAsync("LCOMP?", false) == "ON";
            }
        }
    }

    public class ZCompCommand : AttachedCommand
    {
        private bool zComp;

        public ZCompCommand(Device device)
            : base(device)
        {
        }

        public override AutoExecuteType AutoExecute => AutoExecuteType.AfterValue;

        public bool ZComp
        {
            get
            {
                return zComp;
            }
            set
            {
                zComp = value;
                OnPropertyChanged();
            }
        }

        public override async Task Process()
        {
            string command = $"ZCOMP {(ZComp ? "WIRE2" : "NONE")};*OPC?";

            if (!await Device.QueryAction(command, false))
            {
                ZComp = false;
            }
            else
            {
                ZComp = await Device.QueryAsync("ZCOMP?", false) == "WIRE2";
            }
        }
    }

    public class Fluke5520FrontPanelViewModel : FrontPanelViewModel
    {
        public Fluke5520FrontPanelViewModel(Device device)
            : base(device)
        {
            LComp = new LCompCommand(device);
            ZComp = new ZCompCommand(device);

            if (Device.Functions.ContainsKey(Mode.SetACI))
            {
                Device.Functions[Mode.SetACI].AttachedCommands.Add(LComp);
            }
            if (Device.Functions.ContainsKey(Mode.SetACP))
            {
                Device.Functions[Mode.SetACP].AttachedCommands.Add(LComp);
            }
            if (Device.Functions.ContainsKey(Mode.SetRES2W))
            {
                Device.Functions[Mode.SetRES2W].AttachedCommands.Add(ZComp);
            }
            if (Device.Functions.ContainsKey(Mode.SetCAP2W))
            {
                Device.Functions[Mode.SetCAP2W].AttachedCommands.Add(ZComp);
            }
        }

        public override FrontPanelType Type => FrontPanelType.Fluke5520;

        public LCompCommand LComp { get; }

        public ZCompCommand ZComp { get; }
    }
}
