using MetroAutomation.Calibration;
using MetroAutomation.ViewModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MetroAutomation.FrontPanel
{
    public abstract class FrontPanelViewModel : INotifyPropertyChanged
    {
        private Mode functionMode;
        private Function selectedFunction;
        private FunctionProtocol selectedProtocol;
        private BaseValueInfo selectedRange;

        public FrontPanelViewModel(Device device)
        {
            Device = device;
            AvailableModes = Device.Configuration.ModeInfo.Where(x => x.IsAvailable).Select(x => x.Mode).ToArray();

            ProcessCommand = new AsyncCommandHandler(() => SelectedFunction?.Process());

            foreach (var mode in AvailableModes)
            {
                Protocols.Add(mode, FrontPanelUtils.GetProtocol(Device, mode));
            }

            FunctionMode = AvailableModes.FirstOrDefault();
            SelectedFunction = Device.Functions[FunctionMode];

            Task.Factory.StartNew(ProcessingLoop, TaskCreationOptions.LongRunning);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public abstract FrontPanelType Type { get; }

        public FrontPanelViewModel Base => this;

        public Device Device { get; }

        public Mode[] AvailableModes { get; }

        public Dictionary<Mode, FunctionProtocol> Protocols { get; }
            = new Dictionary<Mode, FunctionProtocol>();

        public Mode FunctionMode
        {
            get
            {
                return functionMode;
            }
            set
            {
                functionMode = value;
                SelectedFunction = Device.Functions[FunctionMode];
                SelectedProtocol = Protocols[FunctionMode];
                OnPropertyChanged();
            }
        }

        public Function SelectedFunction
        {
            get
            {
                return selectedFunction;
            }
            private set
            {
                selectedFunction = value;
                OnPropertyChanged();

                selectedRange = SelectedFunction == null ? null : new BaseValueInfo(SelectedFunction.Range);
                OnPropertyChanged(nameof(SelectedRange));

                RunProcess();
            }
        }

        public FunctionProtocol SelectedProtocol
        {
            get
            {
                return selectedProtocol;
            }
            set
            {
                selectedProtocol = value;
                OnPropertyChanged();
            }
        }

        public BaseValueInfo SelectedRange
        {
            get
            {
                return selectedRange;
            }
            set
            {
                selectedRange = value;

                if (SelectedFunction != null && SelectedRange != null)
                {
                    SelectedFunction.Range.FromValueInfo(SelectedRange, true);
                }

                OnPropertyChanged();

                RunProcess();
            }
        }

        public IAsyncCommand ProcessCommand { get; }

        public bool IsInfiniteReading { get; set; }

        private async void RunProcess()
        {
            await ProcessCommand.ExecuteAsync(null);
        }

        private async void ProcessingLoop()
        {
            while (true)
            {
                if (!ProcessCommand.IsProcessing && IsInfiniteReading)
                {
                    await SelectedFunction?.Process();
                }

                await Task.Delay(100);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
