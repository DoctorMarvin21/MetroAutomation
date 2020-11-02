using MahApps.Metro.Controls.Dialogs;
using MetroAutomation.Model;
using MetroAutomation.ViewModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MetroAutomation.Automation
{
    public class DeviceProtocolManager : INotifyPropertyChanged
    {
        private DeviceProtocol deviceProtocolCopy;
        private DeviceProtocol deviceProtocol;
        private bool isProtocolLoaded;

        public DeviceProtocolManager(MainViewModel owner)
        {
            Owner = owner;
            NewProtocolCommand = new AsyncCommandHandler(NewProtocol);
            OpenProtocolCommand = new AsyncCommandHandler(OpenProtocol);
            SaveProtocolCommand = new CommandHandler(SaveProtocol);
            CloseProtocolCommand = new AsyncCommandHandler(CloseProtocol);
            SaveClicheCommand = new CommandHandler(SaveCliche);
            ApplyClicheCommand = new CommandHandler(ApplyCliche);
            ExportToRtfCommand = new CommandHandler(() => ExportToRtf(true));
            ExportToRtfWithoutUnitsCommand = new CommandHandler(() => ExportToRtf(false));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel Owner { get; }

        public DeviceProtocol DeviceProtocol
        {
            get
            {
                return deviceProtocol;
            }
            private set
            {
                deviceProtocol = value;
                deviceProtocolCopy = deviceProtocol?.BinaryDeepClone();
                deviceProtocol?.Initialize(Owner);
                OnPropertyChanged();

                IsProtocolLoaded = DeviceProtocol != null;
            }
        }

        public bool IsProtocolLoaded
        {
            get
            {
                return isProtocolLoaded;
            }
            private set
            {
                isProtocolLoaded = value;
                OnPropertyChanged();
            }
        }

        public IAsyncCommand NewProtocolCommand { get; }

        public ICommand SaveProtocolCommand { get; }

        public ICommand OpenProtocolCommand { get; }

        public IAsyncCommand CloseProtocolCommand { get; }

        public ICommand SaveClicheCommand { get; }

        public ICommand ApplyClicheCommand { get; }

        public ICommand ExportToRtfCommand { get; }

        public ICommand ExportToRtfWithoutUnitsCommand { get; }

        public bool CanSaveProtocol { get; set; }

        private async Task NewProtocol()
        {
            if (!await SaveCurrentProtocol())
            {
                return;
            }

            await CloseWithoutPromt();

            DeviceProtocol = new DeviceProtocol();
        }

        private async Task OpenProtocol()
        {
            if (!await SaveCurrentProtocol())
            {
                return;
            }

            await CloseWithoutPromt();

            OpenProtocolDialog protocolDialog = new OpenProtocolDialog(this);

            if (protocolDialog.ShowDialog() == true && protocolDialog.DeviceProtocols.SelectedItem != null)
            {
                DeviceProtocol = LiteDBAdaptor.LoadData<DeviceProtocol>(protocolDialog.DeviceProtocols.SelectedItem.ID);
            }
        }

        private void SaveProtocol()
        {
            if (DeviceProtocol != null)
            {
                DeviceProtocol.PrepareToStore(false);
                LiteDBAdaptor.SaveData(DeviceProtocol);
                deviceProtocolCopy = DeviceProtocol.BinaryDeepClone();
            }
        }

        private async Task CloseProtocol()
        {
            if (DeviceProtocol != null && await SaveCurrentProtocol())
            {
                await CloseWithoutPromt();
            }
        }

        private async Task CloseWithoutPromt()
        {
            if (DeviceProtocol != null)
            {
                DeviceProtocol = null;
                var controller = await Owner.Owner.ShowProgressAsync("Отключение оборудования", "Подождите, идёт отключение неиспользуемого оборудования...");
                await Owner.ConnectionManager.DisconnectAndUnloadUnusedDevices();
                await controller.CloseAsync();
            }
        }

        public bool ShouldBeSaved()
        {
            if (deviceProtocolCopy == null || DeviceProtocol == null)
            {
                return false;
            }
            else
            {
                DeviceProtocol.PrepareToStore(false);
                return !DeviceProtocol.DeepBinaryEquals(deviceProtocolCopy);
            }
        }

        public async Task<bool> SaveCurrentProtocol()
        {
            if (!ShouldBeSaved())
            {
                return true;
            }

            var result = await Owner.Owner.ShowMessageAsync("Сохранить",
            "Сохранить текущий протокол?",
            MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary,
            new MetroDialogSettings
            {
                DefaultButtonFocus = MessageDialogResult.Affirmative,
                AffirmativeButtonText = "Да",
                NegativeButtonText = "Нет",
                FirstAuxiliaryButtonText = "Отмена"
            });

            if (result == MessageDialogResult.Affirmative)
            {
                SaveProtocol();
                return true;
            }
            else if (result == MessageDialogResult.Negative)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SaveCliche()
        {
            if (DeviceProtocol != null)
            {
                OpenClicheDialog(DeviceProtocol.ToCliche());
            }
        }

        private void ApplyCliche()
        {
            OpenClicheDialog(null);
        }

        private void OpenClicheDialog(DeviceProtocolCliche cliche)
        {
            if (DeviceProtocol != null)
            {
                OpenClicheDialog clicheDialog = new OpenClicheDialog(cliche);
                if (clicheDialog.ShowDialog() == true && clicheDialog.ProtocolCliche.SelectedItem != null)
                {
                    var newCliche = LiteDBAdaptor.LoadData<DeviceProtocolCliche>(clicheDialog.ProtocolCliche.SelectedItem.ID);
                    DeviceProtocol.FromCliche(newCliche);
                }
            }
        }

        private void ExportToRtf(bool includeUntis)
        {
            if (DeviceProtocol != null)
            {
                var document = ReportGenerator.ToDocument(DeviceProtocol, includeUntis);
                DocumentPreviewWindow previewWindow = new DocumentPreviewWindow(document);
                previewWindow.Show();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
