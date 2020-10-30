using MahApps.Metro.Controls.Dialogs;
using MetroAutomation.Calibration;
using MetroAutomation.Controls;
using MetroAutomation.ViewModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MetroAutomation.Automation
{
    public class AutomationProcessor : INotifyPropertyChanged
    {
        private bool isProcessing;

        private bool isStopRequested;
        private int progress;
        private int count = 1;

        public AutomationProcessor(DeviceProtocol protocol)
        {
            Owner = protocol;
            StartCommand = new AsyncCommandHandler(Process);
            StopCommand = new CommandHandler(Stop);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public DeviceProtocol Owner { get; set; }

        public IAsyncCommand StartCommand { get; }

        public ICommand StopCommand { get; }

        public int Progress
        {
            get
            {
                return progress;
            }
            private set
            {
                progress = value;
                OnPropertyChanged();
            }
        }

        public int Count
        {
            get
            {
                return count;
            }
            private set
            {
                count = value;
                OnPropertyChanged();
            }
        }

        public bool CanStart => !isProcessing;

        public bool CanStop => isProcessing && !isStopRequested;

        private async Task Process()
        {
            var count = Owner.BindableBlocks.SelectMany(x => x.BindableItems).Count(x => x.IsSelected);

            if (count == 0)
            {
                return;
            }

            Progress = 0;
            Count = count;

            isStopRequested = false;
            isProcessing = true;
            OnPropertyChanged(string.Empty);

            await Owner.Owner.DisconnectUnusedDevices();

            var usedConnections = Owner.GetUsedConnections();

            foreach (var connection in usedConnections)
            {
                await connection.Connect();

                if (connection.Device.IsOutputOn)
                {
                    await connection.Device.ChangeOutput(false, false);
                }

                connection.Device.ResetRangeAndMode();
                connection.Device.OnRangeChanged = ProcessRangeChanged;
            }

            foreach (var block in Owner.BindableBlocks)
            {
                foreach (var item in block.BindableItems)
                {
                    if (item.IsSelected)
                    {
                        Progress++;
                    }

                    if (item.IsSelected && !item.HasErrors)
                    {
                        item.Status = LedState.Warn;

                        if (!await item.ProcessFunction())
                        {
                            item.Status = LedState.Fail;
                        }
                    }

                    if (isStopRequested)
                    {
                        break;
                    }
                }

                if (isStopRequested)
                {
                    break;
                }
            }

            foreach (var connection in usedConnections)
            {
                connection.Device.OnRangeChanged = null;

                if (connection.Device.IsOutputOn)
                {
                    await connection.Device.ChangeOutput(false, false);
                }
            }

            isProcessing = false;
            isStopRequested = false;

            OnPropertyChanged(string.Empty);

            Progress = Count;
        }

        private void Stop()
        {
            isStopRequested = true;
            OnPropertyChanged(nameof(CanStop));
        }

        private async Task<bool> ProcessRangeChanged(RangeInfo lastRange, Function function)
        {
            var window = Owner.Owner.Owner;

            if (function.RangeInfo.Output != lastRange?.Output)
            {
                string outputType = function.Direction == Direction.Get ? "входу" : "выходу";

                var result = await window.ShowMessageAsync(
                    $"Подключение", $"Подключитесь ко {outputType} \"{function.RangeInfo.Output}\" прибора \"{function.Device.Configuration.Name}\"",
                    MessageDialogStyle.AffirmativeAndNegative,
                    new MetroDialogSettings
                    {
                        AffirmativeButtonText = "ОК",
                        NegativeButtonText = "Отмена",
                        DefaultButtonFocus = MessageDialogResult.Affirmative
                    });

                if (result != MessageDialogResult.Affirmative)
                {
                    Stop();
                }

                return result == MessageDialogResult.Affirmative;
            }
            else
            {
                return true;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
