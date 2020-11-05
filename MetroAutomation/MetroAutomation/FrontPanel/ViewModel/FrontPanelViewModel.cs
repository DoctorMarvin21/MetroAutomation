using MetroAutomation.Calibration;
using MetroAutomation.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MetroAutomation.FrontPanel
{
    public abstract partial class FrontPanelViewModel : IDisposable, INotifyPropertyChanged
    {
        private readonly SemaphoreSlim loopSemaphore = new SemaphoreSlim(1, 1);
        private bool blockRequests;
        private Mode functionMode;
        private Function selectedFunction;
        private FunctionProtocol selectedProtocol;
        private BaseValueInfo selectedRange;

        private bool isInfiniteReading;

        public FrontPanelViewModel(Device device)
        {
            Device = device;
            device.ConnectionChanged += DeviceConnectionChanged;
            AvailableModes = Device.Configuration.ModeInfo?.Where(x => x.IsAvailable).Select(x => x.Mode).ToArray() ?? new Mode[0];

            OutputOnCommand = new AsyncCommandHandler(() => Device.ChangeOutput(true, false));
            OutputOffCommand = new AsyncCommandHandler(() => Device.ChangeOutput(false, false));
            ToggleOutputCommand = new AsyncCommandHandler(() => Device.ChangeOutput(!Device.IsOutputOn, false));

            foreach (var mode in AvailableModes)
            {
                Protocols.Add(mode, FrontPanelUtils.GetProtocol(Device, mode));
            }

            if (AvailableModes.Length > 0)
            {
                FunctionMode = AvailableModes.FirstOrDefault();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public abstract FrontPanelType Type { get; }

        public FrontPanelViewModel Base => this;

        public Device Device { get; }

        public Mode[] AvailableModes { get; }

        public Dictionary<Mode, FunctionProtocol> Protocols { get; }
            = new Dictionary<Mode, FunctionProtocol>();

        public bool BlockRequests
        {
            get
            {
                return blockRequests;
            }
            set
            {
                blockRequests = value;
                OnPropertyChanged();
            }
        }

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
                var oldFunction = selectedFunction;

                selectedFunction = value;
                OnPropertyChanged();

                selectedRange = SelectedFunction == null ? null : new BaseValueInfo(SelectedFunction.Range);
                OnPropertyChanged(nameof(SelectedRange));

                _ = OnFunctionChanged(oldFunction, selectedFunction);
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
                var oldRange = selectedRange;

                selectedRange = value;

                RefreshRange();

                OnPropertyChanged();

                _ = OnRangeChanged(oldRange, SelectedRange);
            }
        }

        public bool IsInfiniteReading
        {
            get
            {
                return isInfiniteReading;
            }
            set
            {
                isInfiniteReading = value;

                if (value)
                {
                    ProcessingLoop();
                }

                OnPropertyChanged();
            }
        }

        public IAsyncCommand OutputOnCommand { get; }

        public IAsyncCommand OutputOffCommand { get; }

        public IAsyncCommand ToggleOutputCommand { get; }

        protected virtual async Task OnFunctionChanged(Function oldFunction, Function newFunction)
        {
            if (!BlockRequests)
            {
                await (newFunction?.Process() ?? Task.CompletedTask);
            }
        }

        protected virtual async Task OnRangeChanged(BaseValueInfo oldRange, BaseValueInfo newRange)
        {
            if (!BlockRequests)
            {
                await (SelectedFunction?.Process() ?? Task.CompletedTask);
            }
        }

        protected virtual async Task OnConnectionChangedChanged(bool isConnected)
        {
            if (isConnected && !BlockRequests)
            {
                await (SelectedFunction?.Process() ?? Task.CompletedTask);
            }
        }

        public virtual void Refresh()
        {
            RefreshRange();
        }

        private void RefreshRange()
        {
            if (SelectedFunction != null && SelectedRange != null)
            {
                SelectedFunction.Range.FromValueInfo(SelectedRange, true);
            }
        }

        private async void ProcessingLoop()
        {
            try
            {
                await loopSemaphore.WaitAsync();

                while (IsInfiniteReading)
                {
                    if (!Device.IsProcessing && !BlockRequests)
                    {
                        await (SelectedFunction?.ProcessBackground() ?? Task.CompletedTask);
                    }

                    await Task.Delay(100);
                }

                loopSemaphore.Release();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        public DeviceValueSet ToValueSet()
        {
            var values = Protocols
                .Select(x => x.Value.ToValueSet())
                .Where(x => x != null).ToArray();

            if (values.Length == 0)
            {
                return null;
            }
            else
            {
                return new DeviceValueSet
                {
                    ConfigurationID = Device.ConfigurationID,
                    Values = values
                };
            }
        }

        public void FromValueSet(DeviceValueSet valueSet)
        {
            if (valueSet.Values != null)
            {
                foreach (var set in valueSet.Values)
                {
                    if (Protocols.TryGetValue(set.Mode, out FunctionProtocol protocol))
                    {
                        protocol.FromValueSet(set);
                    }
                }
            }
        }

        public void ClearProtocols()
        {
            foreach (var protocol in Protocols)
            {
                protocol.Value.Items.Clear();
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
                case FrontPanelType.Fluke5520:
                    {
                        return new Fluke5520FrontPanelViewModel(device);
                    }
                case FrontPanelType.Fluke8508:
                    {
                        return new Fluke8508FrontPanelViewModel(device);
                    }
                case FrontPanelType.Agilent4980A:
                    {
                        return new AgilentE4980AFrontPanelViewModel(device);
                    }
                case FrontPanelType.Fluke52120A:
                    {
                        return new Fluke52120AFrontPanelViewModel(device);
                    }
                default:
                    {
                        return null;
                    }
            }
        }

        private async void DeviceConnectionChanged(object sender, DeviceConnectionChangedEventArgs e)
        {
            if (e.Status == ConnectionStatus.Connected || e.Status == ConnectionStatus.Disconnected)
            {
                await OnConnectionChangedChanged(e.IsConnected);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            IsInfiniteReading = false;
            loopSemaphore.Dispose();
        }
    }
}
