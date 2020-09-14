using System.Windows;
using System.Windows.Controls;

namespace MetroAutomation.FrontPanel
{
    /// <summary>
    /// Interaction logic for BaseFrontPanel.xaml
    /// </summary>
    public partial class BaseFrontPanel : UserControl
    {
        public static readonly DependencyProperty CalibratorViewModelProperty =
            DependencyProperty.Register(
            nameof(ViewModel), typeof(FrontPanelViewModel),
            typeof(BaseFrontPanel));

        public static readonly DependencyProperty ExtensionSelectorProperty =
            DependencyProperty.Register(
            nameof(ExtensionSelector), typeof(FunctionTemplateSelector),
            typeof(BaseFrontPanel), new PropertyMetadata(new FunctionTemplateSelector()));

        public BaseFrontPanel()
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
