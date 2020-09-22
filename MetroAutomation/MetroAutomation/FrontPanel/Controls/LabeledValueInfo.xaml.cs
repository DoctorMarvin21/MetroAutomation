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

        public static readonly DependencyProperty CanInvertProperty =
            DependencyProperty.Register(
            nameof(CanInvert), typeof(bool),
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

        public bool CanInvert
        {
            get { return (bool)GetValue(CanInvertProperty); }
            set { SetValue(CanInvertProperty, value); }
        }
    }
}
