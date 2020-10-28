using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MetroAutomation.Automation
{
    /// <summary>
    /// Interaction logic for DeviceProtocolControl.xaml
    /// </summary>
    public partial class DeviceProtocolControl : UserControl
    {
        public static readonly DependencyProperty DeviceProtocolProperty =
            DependencyProperty.Register(
            nameof(DeviceProtocol), typeof(DeviceProtocol),
            typeof(DeviceProtocolControl));

        public DeviceProtocolControl()
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
