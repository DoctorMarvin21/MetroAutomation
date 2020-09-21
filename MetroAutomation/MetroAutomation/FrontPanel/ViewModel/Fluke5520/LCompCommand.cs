using MetroAutomation.Calibration;
using System.Threading.Tasks;

namespace MetroAutomation.FrontPanel
{
    public class LCompCommand : AttachedCommand
    {

        private bool lComp;

        public LCompCommand(Function function)
            : base(function)
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

        public override async Task Process(bool background)
        {
            string command = $"LCOMP {(LComp ? "ON" : "OFF")};*OPC?";

            if (!await Function.Device.QueryAction(command, background))
            {
                LComp = false;
            }
            else
            {
                LComp = await Function.Device.QueryAsync("LCOMP?", background) == "ON";
            }
        }
    }
}
