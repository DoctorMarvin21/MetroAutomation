using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MetroAutomation.Controls
{
    /// <summary>
    /// Interaction logic for EnumSelect.xaml
    /// </summary>
    public partial class EnumSelect : UserControl
    {
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(
            nameof(Source), typeof(IEnumerable),
            typeof(EnumSelect));

        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register(
            nameof(Target), typeof(object),
            typeof(EnumSelect));

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
            nameof(Command), typeof(ICommand),
            typeof(EnumSelect));

        public EnumSelect()
        {
            InitializeComponent();
        }

        public IEnumerable Source
        {
            get { return (IEnumerable)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public object Target
        {
            get { return GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
    }
}
