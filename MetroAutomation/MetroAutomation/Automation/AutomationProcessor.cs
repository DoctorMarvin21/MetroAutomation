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
        private bool isAnySelected;

        private int progress;
        private int count = 1;

        public AutomationProcessor(DeviceProtocol protocol)
        {
            Owner = protocol;
            StartCommand = new AsyncCommandHandler(Process);
            StopCommand = new CommandHandler(Stop);

            Owner.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(DeviceProtocol.IsSelected))
                {
                    isAnySelected = GetSelectedCount() > 0;
                    OnPropertyChanged(nameof(CanStart));
                }
            };

            isAnySelected = GetSelectedCount() > 0;
            OnPropertyChanged(nameof(CanStart));
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

        public bool CanStart => !IsProcessing && isAnySelected;

        public bool IsProcessing
        {
            get
            {
                return isProcessing;
            }
            set
            {
                isProcessing = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanStart));
                OnPropertyChanged(nameof(CanStop));
            }
        }

        public bool CanStop => IsProcessing && !IsStopRequested;

        public bool IsStopRequested
        {
            get
            {
                return isStopRequested;
            }
            set
            {
                isStopRequested = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanStop));
            }
        }

        private async Task Process()
        {
            var count = GetSelectedCount();

            if (count == 0)
            {
                return;
            }

            Progress = 0;
            Count = count;

            IsStopRequested = false;
            IsProcessing = true;

            await Owner.Owner.ConnectionManager.DisconnectAndUnloadUnusedDevices();

            Owner.Owner.FrontPanelManager.Lock();

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

            if (usedConnections.All(x => x.IsConnected))
            {
                foreach (var block in Owner.BindableBlocks)
                {
                    if (block.IsEnabled)
                    {
                        foreach (var item in block.BindableItems)
                        {
                            if (item.IsSelected && !item.HasErrors)
                            {
                                item.Status = LedState.Warn;

                                if (!await item.ProcessFunction())
                                {
                                    item.Status = LedState.Fail;
                                }
                            }

                            if (item.IsSelected)
                            {
                                Progress++;
                            }

                            if (IsStopRequested)
                            {
                                break;
                            }
                        }
                    }

                    if (IsStopRequested)
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
            }

            Owner.Owner.FrontPanelManager.Unlock();

            IsProcessing = false;
            IsStopRequested = false;

            Progress = Count;
        }

        private void Stop()
        {
            IsStopRequested = true;
        }

        private int GetSelectedCount()
        {
            return Owner.BindableBlocks.Where(x => x.IsEnabled).SelectMany(x => x.BindableItems).Count(x => x.IsSelected);
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
