using MetroAutomation.Calibration;
using MetroAutomation.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MetroAutomation.Controls
{
    /// <summary>
    /// Interaction logic for ValueInfoTextBox.xaml
    /// </summary>
    public partial class ValueInfoTextBox : UserControl
    {
        public static readonly DependencyProperty ValueInfoProperty =
            DependencyProperty.Register(
            nameof(ValueInfo), typeof(BaseValueInfo),
            typeof(ValueInfoTextBox), new PropertyMetadata(null, ValueChanged));

        public static readonly DependencyProperty SelectedDiscreteValueProperty =
            DependencyProperty.Register(
            nameof(SelectedDiscreteValue), typeof(ActualValueInfo),
            typeof(ValueInfoTextBox), new PropertyMetadata(null, DiscreteValueChanged));

        public static readonly DependencyProperty IsDiscreteProperty =
            DependencyProperty.Register(
            nameof(IsDiscrete), typeof(bool),
            typeof(ValueInfoTextBox));

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(
            nameof(IsReadOnly), typeof(bool),
            typeof(ValueInfoTextBox));

        public ValueInfoTextBox()
        {
            InitializeComponent();
        }

        public BaseValueInfo ValueInfo
        {
            get { return (BaseValueInfo)GetValue(ValueInfoProperty); }
            set { SetValue(ValueInfoProperty, value); }
        }

        public ActualValueInfo SelectedDiscreteValue
        {
            get { return (ActualValueInfo)GetValue(SelectedDiscreteValueProperty); }
            set { SetValue(SelectedDiscreteValueProperty, value); }
        }

        public BindableCollection<Tuple<string, Unit, UnitModifier>> SuggestSource { get; }
            = new BindableCollection<Tuple<string, Unit, UnitModifier>>();

        public BindableCollection<ActualValueInfo> DiscreteValues { get; }
            = new BindableCollection<ActualValueInfo>();

        public bool IsDiscrete
        {
            get { return (bool)GetValue(IsDiscreteProperty); }
            set { SetValue(IsDiscreteProperty, value); }
        }

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var owner = (ValueInfoTextBox)d;
            owner.ValueInfo?.UpdateText();
            owner.DiscreteValues.Clear();

            if (owner.ValueInfo is ValueInfo info)
            {
                owner.IsDiscrete = info.IsDiscrete;
                owner.IsReadOnly = info.IsReadOnly;
                
                if (info.DiscreteValues?.Length > 0)
                {
                    foreach (var value in info.DiscreteValues)
                    {
                        owner.DiscreteValues.Add(value);
                    }
                }

                owner.SelectedDiscreteValue = new ActualValueInfo(owner.ValueInfo);
            }
            else
            {
                owner.IsDiscrete = false;
                owner.IsReadOnly = false;

                owner.SelectedDiscreteValue = null;
            }
        }

        private static void DiscreteValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var owner = (ValueInfoTextBox)d;

            if (owner.ValueInfo != null && owner.SelectedDiscreteValue != null && !owner.SelectedDiscreteValue.Equals(owner.ValueInfo))
            {
                owner.ValueInfo.FromValueInfo(owner.SelectedDiscreteValue.Value, true);
            }
        }

        private void ValueTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            AutoCompleteList.IsOpen = false;
        }

        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (!AutoCompleteList.IsOpen)
            {
                ValueInfo?.UpdateText();
            }
        }

        private void TextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = (TextBox)sender;

            if (textBox.IsReadOnly)
            {
                return;
            }

            if (e.Key == Key.Enter)
            {
                ValueInfo?.UpdateText();
                textBox.SelectAll();
            }
            else if (e.Key == Key.Space)
            {
                if (ValueInfo == null)
                {
                    return;
                }

                e.Handled = true;

                FillSuggestions();

                var selectionStart = textBox.SelectionStart;
                textBox.Text = textBox.Text.Insert(selectionStart, " ");
                textBox.SelectionStart = selectionStart + 1;

                OpenAutoComplete();
            }
            else if (e.Key == Key.Up || e.Key == Key.Down)
            {
                if (ValueInfo == null)
                {
                    return;
                }

                FillSuggestions();

                textBox.Text = (ValueInfo.Value?.ToString() ?? "0") + " ";
                textBox.SelectionStart = textBox.Text.Length;
                textBox.SelectionLength = 0;

                OpenAutoComplete();
            }
        }

        private void FillSuggestions()
        {
            SuggestSource.Clear();

            Unit[] units;

            if (ValueInfo is ValueInfo fullInfo)
            {
                units = FunctionDescription.GetDescription(fullInfo).AllowedUnits;
            }
            else
            {
                units = new[] { ValueInfo.Unit };
            }

            var suggests = ValueInfoUtils.GetUnits(units);

            foreach (var suggest in suggests)
            {
                SuggestSource.Add(Tuple.Create(suggest.Item1, suggest.Item2, suggest.Item3));
            }

            SuggestSource.SelectedItem = SuggestSource.FirstOrDefault(x => x.Item2 == ValueInfo.Unit && x.Item3 == ValueInfo.Modifier);
        }

        private void OpenAutoComplete()
        {
            var offset = ValueTextBox.GetRectFromCharacterIndex(ValueTextBox.SelectionStart).Left;
            AutoCompleteList.PlacementRectangle = new Rect(offset, 1, 1, 0);

            AutoCompleteList.IsOpen = true;
        }

        private void ListBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.Left)
            {
                BackToTextBox(false);
            }
            else if (e.Key == Key.Enter || e.Key == Key.Right)
            {
                BackToTextBox(true);

                if (e.Key == Key.Enter)
                {
                    ValueTextBox.SelectAll();
                }
            }
        }

        private void ListBoxItemSelected(object sender, RoutedEventArgs e)
        {
            var item = (ListBoxItem)sender;
            item.Focus();
        }

        private void ListBoxItemPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BackToTextBox(true);
        }

        private void BackToTextBox(bool updateUnit)
        {
            if (updateUnit)
            {
                int oldTextLenth = ValueTextBox.Text.Length;
                int oldIndex = ValueTextBox.SelectionStart;

                ValueInfo.Unit = SuggestSource.SelectedItem.Item2;
                ValueInfo.Modifier = SuggestSource.SelectedItem.Item3;

                ValueTextBox.SelectionStart = oldIndex + ValueTextBox.Text.Length - oldTextLenth;
            }

            AutoCompleteList.IsOpen = false;
            ValueTextBox.Focus();
        }
    }
}
