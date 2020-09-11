using MetroAutomation.Calibration;
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

        protected override void OnClosing(CancelEventArgs e)
        {
            if (DialogResult == true)
            {
                ((MainWindow)Application.Current.MainWindow).ViewModel.ConnectionManager.UpdateConfigurations(Item);
            }

            base.OnClosing(e);
        }
    }
}
