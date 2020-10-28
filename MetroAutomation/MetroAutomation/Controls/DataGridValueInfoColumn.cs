using MahApps.Metro.Controls;
using MetroAutomation.Calibration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MetroAutomation.Controls
{
    public class DataGridValueInfoColumn : DataGridBoundColumn
    {
        protected override object PrepareCellForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {
            var element = (ValueInfoTextBox)editingElement;

            if (element.ValueInfo is ValueInfo fullInfo && fullInfo.IsDiscrete)
            {
                element.ValueComboBox.Focus();
            }
            else
            {
                element.ValueTextBox.Focus();
            }

            return base.PrepareCellForEdit(editingElement, editingEventArgs);
        }

        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            var result = new ValueInfoTextBox
            {
                BorderThickness = new Thickness(0),
                Margin = new Thickness(0)
            };

            result.ValueTextBox.Style = (Style)Application.Current.Resources["MahApps.Styles.TextBox.DataGrid.Editing"];
            result.ValueTextBox.SetValue(TextBoxHelper.ButtonWidthProperty, 28d);

            result.ValueComboBox.Style = (Style)Application.Current.Resources["MahApps.Styles.ComboBox.DataGrid.Editing"];

            result.SetBinding(ValueInfoTextBox.ValueInfoProperty, Binding);

            return result;
        }

        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.SetBinding(TextBlock.TextProperty, new Binding($"{((Binding)Binding).Path.Path}.{nameof(BaseValueInfo.TextValue)}"));
            return textBlock;
        }
    }
}
