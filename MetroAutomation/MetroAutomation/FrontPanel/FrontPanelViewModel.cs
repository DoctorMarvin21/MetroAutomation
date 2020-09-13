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
            device.ConnectionChanged += DeviceConnectionChanged;
            AvailableModes = Device.Configuration.ModeInfo.Where(x => x.IsAvailable).Select(x => x.Mode).ToArray();

            ProcessCommand = new AsyncCommandHandler(() => SelectedFunction?.Process());
            OutputOnCommand = new AsyncCommandHandler(() => Device.ChangeOutput(true));
            OutputOffCommand = new AsyncCommandHandler(() => Device.ChangeOutput(false));
            ToggleOutputCommand = new AsyncCommandHandler(() => Device.ChangeOutput(!Device.IsOutputOn));

            foreach (var mode in AvailableModes)
            {
                Protocols.Add(mode, FrontPanelUtils.GetProtocol(Device, mode));
            }

            if (AvailableModes.Length > 0)
            {
                FunctionMode = AvailableModes.FirstOrDefault();
                SelectedFunction = Device.Functions[FunctionMode];
            }

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

        public IAsyncCommand OutputOnCommand { get; }

        public IAsyncCommand OutputOffCommand { get; }

        public IAsyncCommand ToggleOutputCommand { get; }

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
                    await SelectedFunction?.ProcessBackground();
                }

                await Task.Delay(100);
            }
        }

        public static FrontPanelViewModel GetViewModel(FrontPanelType type, Device device)
        {
            switch (type)
            {
                case FrontPanelType.Base:
                    {
                        return new BaseFrontPanelViewModel(device);
                    }
                case FrontPanelType.Fluke8508:
                    {
                        return new Fluke8508FrontPanelViewModel(device);
                    }
                default:
                    {
                        return null;
                    }
            }
        }

        private async void DeviceConnectionChanged(object sender, ConnectionChangedEventArgs e)
        {
            if (e.Status == ConnectionStatus.Connected)
            {
                await SelectedFunction?.Process();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
