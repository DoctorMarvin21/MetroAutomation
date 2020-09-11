using System.Windows;
using System.Windows.Controls;

namespace MetroAutomation.FrontPanel
{
    /// <summary>
    /// Interaction logic for CalibratorFrontPanel.xaml
    /// </summary>
    public partial class CalibratorFrontPanel : UserControl
    {
        public static readonly DependencyProperty CalibratorViewModelProperty =
            DependencyProperty.Register(
            nameof(ViewModel), typeof(FrontPanelViewModel),
            typeof(CalibratorFrontPanel));

        public static readonly DependencyProperty ExtensionSelectorProperty =
            DependencyProperty.Register(
            nameof(ExtensionSelector), typeof(FunctionTemplateSelector),
            typeof(CalibratorFrontPanel), new PropertyMetadata(new FunctionTemplateSelector()));

        public CalibratorFrontPanel()
        {
            InitializeComponent();
        }

        public FrontPanelViewModel ViewModel
        {
            get { return (FrontPanelViewModel)GetValue(CalibratorViewModelProperty); }
            set { SetValue(CalibratorViewModelProperty, value); }
        }

        public FunctionTemplateSelector ExtensionSelector
        {
            get { return (FunctionTemplateSelector)GetValue(ExtensionSelectorProperty); }
            set { SetValue(ExtensionSelectorProperty, value); }
        }
    }
}
