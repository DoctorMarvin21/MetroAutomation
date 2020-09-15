using MetroAutomation.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MetroAutomation.Connection
{
    /// <summary>
    /// Interaction logic for ConnectionsControl.xaml
    /// </summary>
    public partial class ConnectionsControl : UserControl
    {
        public static readonly DependencyProperty ConnectionManagerProperty =
            DependencyProperty.Register(
            nameof(ConnectionManager), typeof(ConnectionManager),
            typeof(ConnectionsControl));

        public ConnectionsControl()
        {
            OpenConnectionManagerCommand = new CommandHandler(OpenConnectionManager);

            InitializeComponent();
        }

        public ICommand OpenConnectionManagerCommand { get; }

        public ConnectionManager ConnectionManager
        {
            get { return (ConnectionManager)GetValue(ConnectionManagerProperty); }
            set { SetValue(ConnectionManagerProperty, value); }
        }

        private void OpenConnectionManager(object arg)
        {
            DeviceConnection selectedDevice;

            if (arg is RoutedEventArgs eventArgs && eventArgs.Source is FrameworkElement element && element.DataContext is DeviceConnection deviceConnection)
            {
                selectedDevice = deviceConnection;
            }
            else
            {
                selectedDevice = null;
            }

            ConnectionDialog connectionDialog = new ConnectionDialog(ConnectionManager, selectedDevice);
            connectionDialog.ShowDialog();
        }
    }
}
