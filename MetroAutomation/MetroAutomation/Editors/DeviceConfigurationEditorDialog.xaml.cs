using MetroAutomation.Calibration;
using MetroAutomation.Model;
using System.ComponentModel;
using System.Windows;

namespace MetroAutomation.Editors
{
    /// <summary>
    /// Interaction logic for DeviceConfigurationEditorDialog.xaml
    /// </summary>
    public partial class DeviceConfigurationEditorDialog : BaseEditorDialog, IItemEditor<DeviceConfiguration>
    {
        public DeviceConfigurationEditorDialog(DeviceConfiguration deviceConfiguration)
            : base(deviceConfiguration)
        {
            Item = deviceConfiguration;

            InitializeComponent();
        }

        public DeviceConfiguration Item { get; }

        protected override async void OnClosing(CancelEventArgs e)
        {
            if (DialogResult == true)
            {
                // Calling on end edit to finalize data
                Item.OnEndEdit();

                Item.CommandSet = LiteDBAdaptor.LoadData<CommandSet>(Item.CommandSetID);
                ((MainWindow)Application.Current.MainWindow).ViewModel.ConnectionManager.UpdateConfigurations(Item);
                await ((MainWindow)Application.Current.MainWindow).ViewModel.RefreshConnections();
            }

            base.OnClosing(e);
        }
    }
}
