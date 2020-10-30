using System.Windows;
using System.Windows.Controls;

namespace MetroAutomation.Automation
{
    /// <summary>
    /// Interaction logic for DeviceInfoControl.xaml
    /// </summary>
    public partial class DeviceInfoControl : UserControl
    {
        public static readonly DependencyProperty DeviceProtocolProperty =
            DependencyProperty.Register(
            nameof(DeviceProtocol), typeof(DeviceProtocol),
            typeof(DeviceInfoControl));

        public DeviceInfoControl()
        {
            InitializeComponent();
        }

        public DeviceProtocol DeviceProtocol
        {
            get { return (DeviceProtocol)GetValue(DeviceProtocolProperty); }
            set { SetValue(DeviceProtocolProperty, value); }
        }
    }
}
