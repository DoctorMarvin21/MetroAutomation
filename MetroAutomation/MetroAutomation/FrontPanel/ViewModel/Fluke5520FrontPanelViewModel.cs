using MetroAutomation.Calibration;
using MetroAutomation.ViewModel;
using System.Threading.Tasks;

namespace MetroAutomation.FrontPanel
{
    public class Fluke5520FrontPanelViewModel : FrontPanelViewModel
    {
        private bool lComp;

        public Fluke5520FrontPanelViewModel(Device device)
            : base(device)
        {
            ToggleLCompCommand = new AsyncCommandHandler(() => UpdateLComp(!LComp));
        }

        public override FrontPanelType Type => FrontPanelType.Fluke5520;

        public IAsyncCommand ToggleLCompCommand { get; }

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

        protected override async Task OnFunctionChanged(Function oldFunction, Function newFunction)
        {
            await base.OnFunctionChanged(oldFunction, newFunction);

            if (newFunction?.Mode == Mode.SetACI)
            {
                await UpdateLComp(LComp);
            }
        }

        private async Task UpdateLComp(bool newValue)
        {
            string commandArg = newValue ? "ON" : "OFF";
            if (await Device.QueryAction($"LCOMP {commandArg};*OPC?", false))
            {
                LComp = newValue;
            }
        }
    }
}
