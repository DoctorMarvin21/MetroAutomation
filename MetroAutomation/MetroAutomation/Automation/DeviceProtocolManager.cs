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

        public bool CanSaveProtocol { get; set; }

        private async Task NewProtocol()
        {
            if (!await SuggestSaveProtocol())
            {
                return;
            }

            DeviceProtocol = new DeviceProtocol();
        }

        private async Task OpenProtocol()
        {
            if (!await SuggestSaveProtocol())
            {
                return;
            }

            OpenProtocolDialog protocolDialog = new OpenProtocolDialog();

            if (protocolDialog.ShowDialog() == true)
            {
                DeviceProtocol = LiteDBAdaptor.LoadData<DeviceProtocol>(protocolDialog.DeviceProtocols.SelectedItem.ID);
            }
        }

        private void SaveProtocol()
        {
            if (DeviceProtocol != null)
            {
                DeviceProtocol.PrepareToStore();
                LiteDBAdaptor.SaveData(DeviceProtocol);
            }
        }

        private async Task CloseProtocol()
        {
            if (DeviceProtocol != null && await SuggestSaveProtocol())
            {
                DeviceProtocol = null;
            }
        }

        private bool IsProtocolChanged()
        {
            if (deviceProtocolCopy == null || DeviceProtocol == null)
            {
                return false;
            }
            else
            {
                DeviceProtocol.PrepareToStore();
                return !DeviceProtocol.DeepBinaryEquals(deviceProtocolCopy);
            }
        }

        private async Task<bool> SuggestSaveProtocol()
        {
            if (!IsProtocolChanged())
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

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
