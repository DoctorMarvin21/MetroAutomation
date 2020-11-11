using MetroAutomation.Controls;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MetroAutomation.Automation
{
    /// <summary>
    /// Interaction logic for ProtocolBlockControl.xaml
    /// </summary>
    public partial class ProtocolBlockControl : UserControl
    {
        public static readonly DependencyProperty ProtocolBlockProperty =
            DependencyProperty.Register(
            nameof(ProtocolBlock), typeof(DeviceProtocolBlock),
            typeof(ProtocolBlockControl), new PropertyMetadata(null, ProtocolBlockChanged));

        public ProtocolBlockControl()
        {
            InitializeComponent();
            DataGrid.IsVisibleChanged += DataGridIsVisibleChanged;
        }

        public DeviceProtocolBlock ProtocolBlock
        {
            get { return (DeviceProtocolBlock)GetValue(ProtocolBlockProperty); }
            set { SetValue(ProtocolBlockProperty, value); }
        }

        private static void ProtocolBlockChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ProtocolBlockControl owner = (ProtocolBlockControl)d;

            if (e.NewValue is DeviceProtocolBlock newblock)
            {
                newblock.BindableItems.CollectionChanged += (s, e) =>
                {
                    if (e.Action == NotifyCollectionChangedAction.Reset)
                    {
                        RefreshTable(owner, newblock);
                    }
                };

                RefreshTable(owner, newblock);
            }
        }

        private void DataGridIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            RefreshTable(this, ProtocolBlock);
        }

        private static void RefreshTable(ProtocolBlockControl owner, DeviceProtocolBlock block)
        {
            if (!owner.DataGrid.IsVisible)
            {
                return;
            }

            owner.DataGrid.Columns.Clear();

            if (ProtocolFunctions.PairedFunctions.TryGetValue(block.AutomationMode, out var modeInfo))
            {
                var isSelectedColumn = (DataGridTemplateColumn)owner.Resources["IsSelectedColumnTemplate"];
                owner.DataGrid.Columns.Add(isSelectedColumn);

                var statusColumn = (DataGridTemplateColumn)owner.Resources["StatusColumnTemplate"];
                owner.DataGrid.Columns.Add(statusColumn);

                var columns = modeInfo.GetBlockHeaders(block);

                for (int i = 0; i < columns.Length; i++)
                {
                    DeviceColumnHeader column = columns[i];
                    DataGridLength width = new DataGridLength(0, DataGridLengthUnitType.Auto);

                    DataGridValueInfoColumn valueInfoColumn = new DataGridValueInfoColumn
                    {
                        Width = width,
                        Header = column.Name,
                        Binding = new Binding($"{nameof(DeviceProtocolItem.Values)}[{column.Index}]")
                    };

                    owner.DataGrid.Columns.Add(valueInfoColumn);
                }

                block.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(DeviceProtocolBlock.ItemInProgress))
                    {
                        owner.DataGrid.ScrollIntoView(block.ItemInProgress);
                    }
                };
            }
        }
    }
}
