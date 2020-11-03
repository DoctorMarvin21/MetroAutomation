using MetroAutomation.Calibration;
using MetroAutomation.Model;
using System.Threading.Tasks;
using System.Windows;

namespace MetroAutomation.FrontPanel
{
    public class Fluke52120AConfiguration
    {
        public int DeviceConfigurationID { get; set; }
    }

    public class Fluke52120AFrontPanelViewModel : FrontPanelViewModel
    {
        private const string ConfigFileName = "Fluke52120A.json";

        private Fluke52120AConfiguration configuration;

        public Fluke52120AFrontPanelViewModel(Device device)
            : base(device)
        {
            AvailableDevices = LiteDBAdaptor.GetStandardNames();

            if (Device.Functions.TryGetValue(Mode.SetDCI, out var dci))
            {
                dci.AttachedCommands.Add(new Fluke52120AAmplificationCommand(this, dci));
            }

            if (Device.Functions.TryGetValue(Mode.SetACI, out var aci))
            {
                aci.AttachedCommands.Add(new Fluke52120AAmplificationCommand(this, aci));
            }

            if (Device.Functions.TryGetValue(Mode.SetDCP, out var dcp))
            {
                dcp.AttachedCommands.Add(new Fluke52120AAmplificationCommand(this, dcp));
            }

            if (Device.Functions.TryGetValue(Mode.SetACP, out var acp))
            {
                acp.AttachedCommands.Add(new Fluke52120AAmplificationCommand(this, acp));
            }

            device.OnOutputChanging = UpdateCalibratorOutputBefore;
            device.OnOutputChanged = UpdateCalibratorOutputAfter;
        }

        public Fluke52120AConfiguration Configuration
        {
            get
            {
                if (configuration == null)
                {
                    configuration = JsonFileReaderWriter.ReadData<Fluke52120AConfiguration>(ConfigFileName);
                }

                return configuration;
            }
            set
            {
                JsonFileReaderWriter.WriteData(ConfigFileName, value);
                configuration = value;
                OnPropertyChanged();
            }
        }

        public int DeviceConfigurationID
        {
            get
            {
                return Configuration.DeviceConfigurationID;
            }
            set
            {
                Configuration = new Fluke52120AConfiguration { DeviceConfigurationID = value };
                OnPropertyChanged();
            }
        }

        public NameID[] AvailableDevices { get; }

        public Device CalibratorDevice
        {
            get
            {
                var mainViewModel = (Application.Current.MainWindow as MainWindow).ViewModel;
                var device = mainViewModel.ConnectionManager.LoadDevice(Configuration.DeviceConfigurationID);
                return device.Device;
            }
        }

        public override FrontPanelType Type => FrontPanelType.Fluke52120A;

        protected override Task OnFunctionChanged(Function oldFunction, Function newFunction)
        {
            return Task.CompletedTask;
        }

        private async Task<bool> UpdateCalibratorOutputBefore(bool isOn)
        {
            if (isOn)
            {
                return await CalibratorDevice.ChangeOutput(isOn, false);
            }
            else
            {
                return true;
            }
        }

        private async Task UpdateCalibratorOutputAfter(bool isOn)
        {
            if (!isOn)
            {
                await CalibratorDevice.ChangeOutput(isOn, false);
            }
        }
    }
}
