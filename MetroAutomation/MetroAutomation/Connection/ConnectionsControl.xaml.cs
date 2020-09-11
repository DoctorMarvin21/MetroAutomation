using System.Windows;
using System.Windows.Controls;

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
            InitializeComponent();
        }

        public ConnectionManager ConnectionManager
        {
            get { return (ConnectionManager)GetValue(ConnectionManagerProperty); }
            set { SetValue(ConnectionManagerProperty, value); }
        }
    }
}
