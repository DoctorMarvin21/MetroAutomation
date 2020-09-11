using System.Windows;
using System.Windows.Controls;

namespace MetroAutomation.FrontPanel
{
    /// <summary>
    /// Interaction logic for RangeSelect.xaml
    /// </summary>
    public partial class RangeSelect : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
            nameof(ViewModel), typeof(FrontPanelViewModel),
            typeof(RangeSelect));

        public RangeSelect()
        {
            InitializeComponent();
        }

        public FrontPanelViewModel ViewModel
        {
            get { return (FrontPanelViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
