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

            var usedConnections = Owner.GetUsedConnections();

            Owner.Owner.FrontPanelManager.Lock(usedConnections);

            foreach (var connection in usedConnections)
            {
                await connection.Connect();

                if (connection.Device.IsOutputOn)
                {
                    await connection.Device.ChangeOutput(false, false);
                }

                connection.Device.ResetRangeAndMode();
                connection.Device.OnManualAction = OnManualAction;
                connection.Device.OnManualResult = OnMeasureInput;
                connection.Device.OnRangeChanged = ProcessRangeChanged;
                connection.Device.OnModeChanged = ProcessModeChanged;
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
                                item.StatusText = "Идёт измерение...";

                                if (!await item.ProcessFunction(Owner.Owner.Owner))
                                {
                                    item.Status = LedState.Fail;
                                    item.StatusText = "Ошибка обработки запроса";
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
                            else if (usedConnections.Any(x => !x.Device.IsConnected))
                            {
                                break;
                            }
                        }
                    }

                    if (IsStopRequested)
                    {
                        break;
                    }
                    else if (usedConnections.Any(x => !x.Device.IsConnected))
                    {
                        break;
                    }
                }

                foreach (var connection in usedConnections)
                {
                    connection.Device.OnRangeChanged = null;
                    connection.Device.OnModeChanged = null;
                    connection.Device.OnManualAction = null;
                    connection.Device.OnManualResult = null;

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
                string outputType = function.Direction == Direction.Get ? "ко входу" : "к выходу";

                var result = await window.ShowMessageAsync(
                    $"Подключение {function?.Device.Configuration.Name}", $"Подключитесь {outputType} \"{function.RangeInfo.Output}\" прибора \"{function.Device.Configuration.Name}\"",
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

        private async Task<bool> ProcessModeChanged(Mode? lastMode, Function function)
        {
            bool result;
            if (function.Device.IsOutputOn)
            {
                result = await function.Device.ChangeOutput(false, false);
            }
            else
            {
                result = true;
            }

            if (!result)
            {
                return result;
            }
            else
            {
                if (function.AutoRange && function.RangeInfo == null)
                {
                    var window = Owner.Owner.Owner;

                    var dialogResult = await window.ShowMessageAsync(
                    $"Подключение {function?.Device.Configuration.Name}", $"Подготовьте прибор \"{function.Device.Configuration.Name}\" для \"{ExtendedDescriptionAttribute.GetDescription(function.Mode, DescriptionType.Full)}\"",
                    MessageDialogStyle.AffirmativeAndNegative,
                    new MetroDialogSettings
                    {
                        AffirmativeButtonText = "ОК",
                        NegativeButtonText = "Отмена",
                        DefaultButtonFocus = MessageDialogResult.Affirmative
                    });

                    if (dialogResult != MessageDialogResult.Affirmative)
                    {
                        Stop();
                    }

                    return dialogResult == MessageDialogResult.Affirmative;
                }
                else
                {
                    return true;
                }
            }
        }

        private async Task<bool> OnManualAction(string command, Function function)
        {
            var window = Owner.Owner.Owner;

            var dialogResult = await window.ShowMessageAsync(
            $"Подключение {function?.Device.Configuration.Name}", command,
            MessageDialogStyle.AffirmativeAndNegative,
            new MetroDialogSettings
            {
                AffirmativeButtonText = "ОК",
                NegativeButtonText = "Отмена",
                DefaultButtonFocus = MessageDialogResult.Affirmative
            });

            if (dialogResult != MessageDialogResult.Affirmative)
            {
                Stop();
            }

            return dialogResult == MessageDialogResult.Affirmative;
        }

        private async Task<decimal?> OnMeasureInput(string command, Function function)
        {
            if (function.Direction != Direction.Get || function.Components.Length != 1)
            {
                return null;
            }

            var window = Owner.Owner.Owner;

            var dialog = new MeasureInputDialog(window, $"Ввод измерений {function?.Device.Configuration.Name}", command, function.Components[0]);
            await window.ShowMetroDialogAsync(dialog);
            await dialog.WaitUntilUnloadedAsync();

            if (dialog.Result == MeasureInputResult.Ok)
            {
                return dialog.Value.GetNormal();
            }
            else
            {
                Stop();
                return null;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
