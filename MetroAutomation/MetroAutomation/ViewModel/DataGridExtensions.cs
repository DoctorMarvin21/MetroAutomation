using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace MetroAutomation.ViewModel
{
    public static class DataGridExtensions
    {
        public static readonly DependencyProperty MultiSelectItemsProperty =
            DependencyProperty.RegisterAttached("MultiSelectItems",
            typeof(IList), typeof(DataGridExtensions),
            new PropertyMetadata(null, MultiSelectItemsPropertyChanged));

        public static IList GetMultiSelectItems(DependencyObject obj)
        {
            return (IList)obj.GetValue(MultiSelectItemsProperty);
        }

        public static void SetMultiSelectItems(DependencyObject obj, IList value)
        {
            obj.SetValue(MultiSelectItemsProperty, value);
        }

        private static void MultiSelectItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                var multiSelected = GetMultiSelectItems(d);
                multiSelected.Clear();

                foreach (var item in dataGrid.SelectedItems)
                {
                    multiSelected.Add(item);
                }

                dataGrid.SelectionChanged += DataGridSelectionChanged;
            }
        }

        private static void DataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(e.OriginalSource is DataGrid))
            {
                return;
            }

            var multiSelected = GetMultiSelectItems(sender as DependencyObject);

            if (e.RemovedItems != null)
            {
                foreach (var item in e.RemovedItems)
                {
                    if (multiSelected.Contains(item))
                    {
                        multiSelected.Remove(item);
                    }
                }
            }

            if (e.AddedItems != null)
            {
                foreach (var item in e.AddedItems)
                {
                    if (!multiSelected.Contains(item))
                    {
                        multiSelected.Add(item);
                    }
                }
            }
        }
    }
}
