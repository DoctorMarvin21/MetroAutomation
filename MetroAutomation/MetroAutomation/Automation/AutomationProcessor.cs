using MetroAutomation.ViewModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MetroAutomation.Automation
{
    public class AutomationProcessor : INotifyPropertyChanged
    {
        private bool isProcessing;

        private bool isStopRequested;

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

        public bool CanStart => !isProcessing;

        public bool CanStop => isProcessing && !isStopRequested;

        private async Task Process()
        {
            isStopRequested = false;
            isProcessing = true;
            OnPropertyChanged(string.Empty);

            await Owner.Owner.DisconnectUnusedDevices();

            var usedConnections = Owner.GetUsedConnections();

            foreach (var connection in usedConnections)
            {
                await connection.Connect();
            }

            foreach (var block in Owner.BindableBlocks)
            {
                foreach (var item in block.BindableItems)
                {
                    if (item.IsSelected && !item.HasErrors)
                    {
                        item.IsProcessing = true;
                        await item.ProcessFunction();
                        item.IsProcessing = false;
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

            isProcessing = false;
            isStopRequested = false;

            OnPropertyChanged(string.Empty);
        }

        private void Stop()
        {
            isStopRequested = true;
            OnPropertyChanged(nameof(CanStop));
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
