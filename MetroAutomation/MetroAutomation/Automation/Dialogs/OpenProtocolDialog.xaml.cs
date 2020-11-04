using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MetroAutomation.Model;
using MetroAutomation.ViewModel;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MetroAutomation.Automation
{
    /// <summary>
    /// Interaction logic for OpenProtocolDialog.xaml
    /// </summary>
    public partial class OpenProtocolDialog : MetroWindow
    {
        private string filter;

        public OpenProtocolDialog(DeviceProtocolManager owner)
        {
            WindowOwner = owner;
            DeviceProtocols.GetInstanceDelegate = GetInstance;
            DeviceProtocols.GetCopyDelegate = GetCopy;
            DeviceProtocols.RemoveDelegate = Remove;

            ExportToRtfCommand = new CommandHandler(() => ExportToRtf(true));
            ExportToRtfWithoutUnitsCommand = new CommandHandler(() => ExportToRtf(false));

            InitializeComponent();

            UpdateCollection();
        }

        public DeviceProtocolManager WindowOwner { get; }

        public ICommand ApplyCommand => new CommandHandler(() => { if (DeviceProtocols.SelectedItem != null) DialogResult = true; });

        public ICommand CancelCommand => new CommandHandler(() => DialogResult = false);

        public ICommand ExportToRtfCommand { get; }

        public ICommand ExportToRtfWithoutUnitsCommand { get; }

        public string Filter
        {
            get
            {
                return filter;
            }
            set
            {
                filter = value;
                UpdateCollection();
            }
        }

        public BindableCollection<IDeviceProtocolDisplayed> DeviceProtocols { get; }
            = new BindableCollection<IDeviceProtocolDisplayed>();

        private IDeviceProtocolDisplayed GetInstance()
        {
            DeviceProtocol deviceProtocol = new DeviceProtocol();
            LiteDBAdaptor.SaveData(deviceProtocol);
            return deviceProtocol;
        }

        private IDeviceProtocolDisplayed GetCopy(IDeviceProtocolDisplayed item)
        {
            var originalItem = LiteDBAdaptor.LoadData<DeviceProtocol>(item.ID);
            var copy = originalItem.BinaryDeepClone();
            copy.ID = Guid.Empty;
            LiteDBAdaptor.SaveData(copy);

            return copy;
        }

        private async Task<bool> Remove(IDeviceProtocolDisplayed item)
        {
            var result = await this.ShowMessageAsync("Удалить",
            $"Вы действительно хотите удалить протокол на \"{item.Name} {item.Type}\"? Данное действие невозможно будет отменить.",
            MessageDialogStyle.AffirmativeAndNegative,
            new MetroDialogSettings
            {
                DefaultButtonFocus = MessageDialogResult.Negative,
                AffirmativeButtonText = "Да",
                NegativeButtonText = "Нет"
            });

            if (result == MessageDialogResult.Affirmative)
            {
                LiteDBAdaptor.RemoveData<DeviceProtocol>(item.ID);
            }

            return result == MessageDialogResult.Affirmative;
        }

        private void ExportToRtf(bool includeUntis)
        {
            if (DeviceProtocols.SelectedItem != null)
            {
                var item = LiteDBAdaptor.LoadData<DeviceProtocol>(DeviceProtocols.SelectedItem.ID);
                item.Initialize(WindowOwner.Owner);

                var document = ReportGenerator.ToDocument(item, includeUntis);

                WindowOwner.Owner.ConnectionManager.UnloadUnusedDisconnectedDevices();

                DocumentPreviewWindow previewWindow = new DocumentPreviewWindow(true, document);
                previewWindow.Show();
            }
        }

        private void UpdateCollection()
        {
            DeviceProtocols.Clear();

            var collection = LiteDBAdaptor.SearchProtocol(100, filter);

            foreach (var item in collection)
            {
                DeviceProtocols.Add(item);
            }
        }
    }
}
