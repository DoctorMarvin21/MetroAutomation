using MetroAutomation.Calibration;

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
    }
}
