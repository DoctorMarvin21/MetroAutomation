using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MetroAutomation.Controls
{
    /// <summary>
    /// Interaction logic for OkCancelButtons.xaml
    /// </summary>
    public partial class OkCancelButtons : UserControl
    {
        public static readonly DependencyProperty OkTextProperty =
            DependencyProperty.Register(
            nameof(OkText), typeof(string),
            typeof(OkCancelButtons), new PropertyMetadata("OK"));

        public static readonly DependencyProperty CancelTextProperty =
            DependencyProperty.Register(
            nameof(CancelText), typeof(string),
            typeof(OkCancelButtons), new PropertyMetadata("Отмена"));

        public static readonly DependencyProperty OkCommandProperty =
            DependencyProperty.Register(
            nameof(OkCommand), typeof(ICommand),
            typeof(OkCancelButtons));

        public static readonly DependencyProperty CancelCommandProperty =
            DependencyProperty.Register(
            nameof(CancelCommand), typeof(ICommand),
            typeof(OkCancelButtons));

        public OkCancelButtons()
        {
            InitializeComponent();
        }

        public string OkText
        {
            get { return (string)GetValue(OkTextProperty); }
            set { SetValue(OkTextProperty, value); }
        }

        public string CancelText
        {
            get { return (string)GetValue(CancelTextProperty); }
            set { SetValue(CancelTextProperty, value); }
        }

        public ICommand OkCommand
        {
            get { return (ICommand)GetValue(OkCommandProperty); }
            set { SetValue(OkCommandProperty, value); }
        }

        public ICommand CancelCommand
        {
            get { return (ICommand)GetValue(CancelCommandProperty); }
            set { SetValue(CancelCommandProperty, value); }
        }
    }
}
