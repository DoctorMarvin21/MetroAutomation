using MetroAutomation.Calibration;
using System.Threading.Tasks;

namespace MetroAutomation.FrontPanel
{
    public class ZCompCommand : AttachedCommand
    {
        private bool zComp;

        public ZCompCommand(Function function)
            : base(function)
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

        public override async Task Process(bool background)
        {
            string command = $"ZCOMP {(ZComp ? "WIRE2" : "NONE")};*OPC?";

            if (!await Function.Device.QueryAction(command, background))
            {
                ZComp = false;
            }
            else
            {
                ZComp = await Function.Device.QueryAsync("ZCOMP?", background) == "WIRE2";
            }
        }
    }
}
