using System.Collections;
using System.Windows;
using System.Windows.Controls;

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
    }
}
