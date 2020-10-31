using MahApps.Metro.Controls;
using MetroAutomation.Model;
using MetroAutomation.ViewModel;
using System.Windows.Input;

namespace MetroAutomation.Automation
{
    /// <summary>
    /// Interaction logic for OpenProtocolDialog.xaml
    /// </summary>
    public partial class OpenProtocolDialog : MetroWindow
    {
        private string filter;

        public OpenProtocolDialog()
        {
            InitializeComponent();

            // TODO: Add get copy, also, override remove
            DeviceProtocols.GetInstanceDelegate = null;
            DeviceProtocols.GetCopyDelegate = null;
            DeviceProtocols.RemoveDelegate = null;

            UpdateCollection();
        }

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

        public BindableCollection<DeviceProtocolDisplayed> DeviceProtocols { get; }
            = new BindableCollection<DeviceProtocolDisplayed>();

        private void UpdateCollection()
        {
            DeviceProtocols.Clear();

            var collection = LiteDBAdaptor.SearchProtocol(100, filter);

            foreach (var item in collection)
            {
                DeviceProtocols.Add(item);
            }
        }

        public ICommand ApplyCommand => new CommandHandler(() => { if (DeviceProtocols.SelectedItem != null) DialogResult = true; });

        public ICommand CancelCommand => new CommandHandler(() => DialogResult = false);
    }
}
