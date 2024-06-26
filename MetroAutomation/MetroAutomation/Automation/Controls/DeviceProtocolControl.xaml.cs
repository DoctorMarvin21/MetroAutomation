﻿using System.Windows;
using System.Windows.Controls;

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
