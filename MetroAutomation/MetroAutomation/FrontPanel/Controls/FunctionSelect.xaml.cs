using System.Windows;
using System.Windows.Controls;

namespace MetroAutomation.FrontPanel
{
    /// <summary>
    /// Interaction logic for FunctionSelect.xaml
    /// </summary>
    public partial class FunctionSelect : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
            nameof(ViewModel), typeof(FrontPanelViewModel),
            typeof(FunctionSelect));

        public FunctionSelect()
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
