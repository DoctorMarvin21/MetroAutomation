using System.Windows;
using System.Windows.Controls;

namespace MetroAutomation.FrontPanel
{
    /// <summary>
    /// Interaction logic for FrontPanelControl.xaml
    /// </summary>
    public partial class FrontPanelControl : UserControl
    {
        public static readonly DependencyProperty FrontPanelManagerProperty =
            DependencyProperty.Register(
            nameof(FrontPanelManager), typeof(FrontPanelManager),
            typeof(FrontPanelControl));

        public FrontPanelControl()
        {
            InitializeComponent();
        }

        public FrontPanelManager FrontPanelManager
        {
            get { return (FrontPanelManager)GetValue(FrontPanelManagerProperty); }
            set { SetValue(FrontPanelManagerProperty, value); }
        }
    }
}
