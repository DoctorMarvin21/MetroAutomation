using MetroAutomation.Calibration;
using MetroAutomation.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MetroAutomation.FrontPanel
{
    /// <summary>
    /// Interaction logic for FunctionProtocolDataGrid.xaml.
    /// </summary>
    public partial class FunctionProtocolDataGrid : UserControl
    {
        public static readonly DependencyProperty ProtocolProperty =
            DependencyProperty.Register(
            nameof(Protocol), typeof(FunctionProtocol),
            typeof(FunctionProtocolDataGrid), new PropertyMetadata(null, ProtocolChanged));

        public FunctionProtocolDataGrid()
        {
            InitializeComponent();
        }

        public FunctionProtocol Protocol
        {
            get { return (FunctionProtocol)GetValue(ProtocolProperty); }
            set { SetValue(ProtocolProperty, value); }
        }

        private static void ProtocolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FunctionProtocolDataGrid owner = (FunctionProtocolDataGrid)d;

            if (e.NewValue is FunctionProtocol newProtocol)
            {
                owner.DataGrid.Columns.Clear();

                var description = FunctionDescription.Components[newProtocol.OriginalFuntion.Mode];

                for (int i = 0; i < description.Length; i++)
                {
                    DataGridValueInfoColumn valueInfoColumn = new DataGridValueInfoColumn
                    {
                        Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                        Header = description[i].FullName,
                        Binding = new Binding($"{nameof(FunctionProtocolItem.Function)}.{nameof(Function.Components)}[{i}]")
                    };
                    owner.DataGrid.Columns.Add(valueInfoColumn);
                }

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
}
