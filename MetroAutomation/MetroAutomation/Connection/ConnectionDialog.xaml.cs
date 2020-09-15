using MahApps.Metro.Controls;
using System.Windows;

namespace MetroAutomation.Connection
{
    /// <summary>
    /// Interaction logic for ConnectionDialog.xaml
    /// </summary>
    public partial class ConnectionDialog : MetroWindow
    {
        public static readonly DependencyProperty SelectedDeviceProperty =
            DependencyProperty.Register(
            nameof(SelectedDevice), typeof(DeviceConnection),
            typeof(ConnectionDialog));

        public ConnectionDialog(ConnectionManager connectionManager, DeviceConnection selectedDevice)
        {
            ConnectionManager = connectionManager;
            SelectedDevice = selectedDevice;

            InitializeComponent();
        }

        public ConnectionManager ConnectionManager { get; }

        public DeviceConnection SelectedDevice
        {
            get { return (DeviceConnection)GetValue(SelectedDeviceProperty); }
            set { SetValue(SelectedDeviceProperty, value); }
        }
    }
}
