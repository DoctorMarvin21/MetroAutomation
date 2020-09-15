using MetroAutomation.Calibration;
using MetroAutomation.Model;
using MetroAutomation.ViewModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MetroAutomation.Connection
{
    /// <summary>
    /// Interaction logic for ConnectionSettingsEditor.xaml
    /// </summary>
    public partial class ConnectionSettingsEditor : UserControl
    {
        public static readonly DependencyProperty ConnectionSettingsProperty =
            DependencyProperty.Register(
            nameof(ConnectionSettings), typeof(ConnectionSettings),
            typeof(ConnectionSettingsEditor));

        private static ConnectedDeviceInfo[] deviceConnections; 

        public ConnectionSettingsEditor()
        {
            RefreshExistingConnectionsCommand = new AsyncCommandHandler(RefreshExistingConnections);
            ApplySelectedConnectionCommand = new CommandHandler(ApplySelectedConnection);

            if (deviceConnections == null)
            {
                _ = RefreshExistingConnections();
            }
            else
            {
                foreach (var device in deviceConnections)
                {
                    ConnectedDevices.Add(device);
                }
            }

            InitializeComponent();
        }

        public IAsyncCommand RefreshExistingConnectionsCommand { get; }

        public ICommand ApplySelectedConnectionCommand { get; }

        public ObservableCollection<ConnectedDeviceInfo> ConnectedDevices { get; }
            = new ObservableCollection<ConnectedDeviceInfo>();

        public ConnectedDeviceInfo SelectedConnectedDevice { get; set; }

        public ConnectionSettings ConnectionSettings
        {
            get { return (ConnectionSettings)GetValue(ConnectionSettingsProperty); }
            set { SetValue(ConnectionSettingsProperty, value); }
        }

        private async Task RefreshExistingConnections()
        {
            ConnectedDevices.Clear();
            SelectedConnectedDevice = null;

            deviceConnections = await Task.Run(VisaComWrapper.GetDevicesList);
            
            foreach (var device in deviceConnections)
            {
                ConnectedDevices.Add(device);
            }

            SelectedConnectedDevice = ConnectedDevices.FirstOrDefault();
        }

        private void ApplySelectedConnection()
        {
            if (ConnectionSettings != null && SelectedConnectedDevice != null)
            {
                ConnectionSettings.AdvancedConnectionSettings = ConnectionUtils.GetConnectionSettingsByResourceName(SelectedConnectedDevice.ResourceName);
            }
        }
    }
}
