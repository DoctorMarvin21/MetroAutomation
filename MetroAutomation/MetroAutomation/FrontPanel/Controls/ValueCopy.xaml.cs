using MetroAutomation.Calibration;
using MetroAutomation.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MetroAutomation.FrontPanel
{
    /// <summary>
    /// Interaction logic for ValueCopy.xaml
    /// </summary>
    public partial class ValueCopy : UserControl
    {
        public static readonly DependencyProperty SuggestSourceProperty =
            DependencyProperty.Register(
            nameof(SuggestSource), typeof(BindableCollection<Tuple<string, Unit, UnitModifier>>),
            typeof(ValueCopy));

        public static readonly DependencyProperty CopyModifiedCommandProperty =
            DependencyProperty.Register(
            nameof(CopyModifiedCommand), typeof(ICommand),
            typeof(ValueCopy));

        public ValueCopy()
        {
            InitializeComponent();
        }

        public BindableCollection<Tuple<string, Unit, UnitModifier>> SuggestSource
        {
            get { return (BindableCollection<Tuple<string, Unit, UnitModifier>>)GetValue(SuggestSourceProperty); }
            set { SetValue(SuggestSourceProperty, value); }
        }

        public ICommand CopyModifiedCommand
        {
            get { return (ICommand)GetValue(CopyModifiedCommandProperty); }
            set { SetValue(CopyModifiedCommandProperty, value); }
        }
    }
}
