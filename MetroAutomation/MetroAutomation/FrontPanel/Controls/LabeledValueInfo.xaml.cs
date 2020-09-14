using MetroAutomation.Calibration;
using System.Windows;
using System.Windows.Controls;

namespace MetroAutomation.FrontPanel
{
    /// <summary>
    /// Interaction logic for LabeledValueInfo.xaml
    /// </summary>
    public partial class LabeledValueInfo : UserControl
    {
        public static readonly DependencyProperty ValueInfoProperty =
            DependencyProperty.Register(
            nameof(ValueInfo), typeof(ValueInfo),
            typeof(LabeledValueInfo));

        public LabeledValueInfo()
        {
            InitializeComponent();
        }

        public ValueInfo ValueInfo
        {
            get { return (ValueInfo)GetValue(ValueInfoProperty); }
            set { SetValue(ValueInfoProperty, value); }
        }
    }
}
