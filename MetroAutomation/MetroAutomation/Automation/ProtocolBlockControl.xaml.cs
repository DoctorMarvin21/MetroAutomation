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

        private static void RefreshTable(ProtocolBlockControl owner, DeviceProtocolBlock block)
        {
            owner.DataGrid.Columns.Clear();

            if (ProtocolFunctions.PairedFunctions.TryGetValue(block.AutomationMode, out var modeInfo))
            {
                var columns = modeInfo.GetBlockHeaders(block);

                for (int i = 0; i < columns.Length; i++)
                {
                    DeviceColumnHeader column = columns[i];

                    DataGridValueInfoColumn valueInfoColumn = new DataGridValueInfoColumn
                    {
                        Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                        Header = column.Name,
                        Binding = new Binding($"{nameof(DeviceProtocolItem.Values)}[{column.Index}]")
                    };

                    owner.DataGrid.Columns.Add(valueInfoColumn);
                }
            }

            //var description = FunctionDescription.Components[newblock.OriginalFuntion.Mode];

            //if (newblock.AvailableMultipliers?.Length > 0)
            //{
            //    var multiplierColumn = (DataGridComboBoxColumn)owner.Resources["MultiplierColumn"];
            //    multiplierColumn.ItemsSource = newblock.AvailableMultipliers;
            //    owner.DataGrid.Columns.Add(multiplierColumn);
            //}

            //for (int i = 0; i < description.Length; i++)
            //{
            //    DataGridValueInfoColumn valueInfoColumn = new DataGridValueInfoColumn
            //    {
            //        Width = new DataGridLength(1, DataGridLengthUnitType.Star),
            //        Header = description[i].FullName,
            //        Binding = new Binding($"{nameof(FunctionProtocolItem.Function)}.{nameof(Function.Components)}[{i}]")
            //    };

            //    owner.DataGrid.Columns.Add(valueInfoColumn);
            //}

            DataGridTemplateColumn buttonColumn = new DataGridTemplateColumn
            {
                Width = 26,
                MinWidth = 26,
                MaxWidth = 26,
                CellTemplate = (DataTemplate)owner.Resources["ActionButtonTemplate"]
            };

            owner.DataGrid.Columns.Add(buttonColumn);
        }
    }
}
