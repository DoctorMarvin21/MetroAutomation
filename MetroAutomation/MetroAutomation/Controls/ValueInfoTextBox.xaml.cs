using MetroAutomation.Calibration;
using MetroAutomation.ViewModel;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
            nameof(Command), typeof(ICommand),
            typeof(ValueInfoTextBox));

        public ValueInfoTextBox()
        {
            Mutltiply10Command = new CommandHandler(Multiply10);
            Divide10Command = new CommandHandler(Divide10);

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

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public ICommand Mutltiply10Command { get; }

        public ICommand Divide10Command { get; }

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
        }

        private static void DiscreteValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var owner = (ValueInfoTextBox)d;

            if (owner.ValueInfo != null && owner.SelectedDiscreteValue != null && !owner.SelectedDiscreteValue.Equals(owner.ValueInfo))
            {
                owner.ValueInfo.FromValueInfo(owner.SelectedDiscreteValue.Value, true);
                owner.Command?.Execute(null);
            }
        }

        private void ValueTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = e.OriginalSource as TextBox;
            if (!IsReadOnly && textBox != null)
            {
                textBox.SelectAll();
            }

            AutoCompleteList.IsOpen = false;
        }

        private void ValueTextBoxPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsReadOnly)
            {
                return;
            }

            // Find the TextBox
            DependencyObject parent = e.OriginalSource as UIElement;

            while (parent != null && !(parent is TextBox))
            {
                parent = VisualTreeHelper.GetParent(parent);

                if (parent is Button)
                {
                    parent = null;
                    break;
                }
            }

            if (parent != null)
            {
                var textBox = (TextBox)parent;
                if (!textBox.IsKeyboardFocusWithin)
                {
                    // If the text box is not yet focused, give it the focus and
                    // stop further processing of this click event.
                    textBox.Focus();
                    e.Handled = true;
                }
            }
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

            if (e.Key == Key.Enter)
            {
                if (ValueInfo == null)
                {
                    return;
                }

                ValueInfo.UpdateText();
                textBox.SelectAll();

                Command?.Execute(null);
            }
            else if (e.Key == Key.Space)
            {
                if (IsReadOnly || ValueInfo == null)
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
                if (IsReadOnly || ValueInfo == null)
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
            if (SuggestSource.Count == 0)
            {
                return;
            }
            else if (SuggestSource.Count == 1 && SuggestSource[0].Item2 == Unit.None)
            {
                return;
            }

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
                    Command?.Execute(null);
                }
            }
            else
            {
                // TODO: doesn't work
                //if (KeyboardHelper.InputText.TryGetValue(e.Key, out string keyText))
                //{
                //    var meets = SuggestSource.Where(x => x.Item1.StartsWith(keyText, StringComparison.OrdinalIgnoreCase)).ToArray();

                //    foreach (var meet in meets)
                //    {
                //        if (SuggestSource.SelectedItem != meet)
                //        {
                //            try
                //            {
                //                SuggestSource.SelectedItem = meet;
                //                break;
                //            }
                //            catch
                //            {

                //            }
                //        }
                //    }
                //}
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
            Command?.Execute(null);
        }

        private void BackToTextBox(bool updateUnit)
        {
            if (updateUnit)
            {
                int oldTextLenth = ValueTextBox.Text.Length;
                int oldIndex = ValueTextBox.SelectionStart;

                ValueInfo.Unit = SuggestSource.SelectedItem.Item2;
                ValueInfo.Modifier = SuggestSource.SelectedItem.Item3;

                int start = oldIndex + ValueTextBox.Text.Length - oldTextLenth;

                if (start < 0)
                {
                    start = 0;
                }
                else if (start > ValueTextBox.Text.Length)
                {
                    start = ValueTextBox.Text.Length;
                }

                ValueTextBox.SelectionStart = start;
            }

            AutoCompleteList.IsOpen = false;
            ValueTextBox.Focus();
        }

        private void Multiply10()
        {
            if (!IsReadOnly && ValueInfo != null)
            {
                ValueInfo.Value *= 10;
                ValueInfo.AutoModifier();

                Command?.Execute(null);

                ValueTextBox.Focus();
                ValueTextBox.SelectAll();
            }
        }

        private void Divide10()
        {
            if (!IsReadOnly && ValueInfo != null)
            {
                ValueInfo.Value /= 10;
                ValueInfo.AutoModifier();

                Command?.Execute(null);

                ValueTextBox.Focus();
                ValueTextBox.SelectAll();
            }
        }
    }
}
