using MetroAutomation.Calibration;
using System.Windows;
using System.Windows.Controls;

namespace MetroAutomation.Controls
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

        public ConnectionSettingsEditor()
        {
            InitializeComponent();
        }

        public ConnectionSettings ConnectionSettings
        {
            get { return (ConnectionSettings)GetValue(ConnectionSettingsProperty); }
            set { SetValue(ConnectionSettingsProperty, value); }
        }
    }
}
