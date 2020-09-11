using MetroAutomation.Calibration;
using MetroAutomation.Controls;
using MetroAutomation.Model;
using MetroAutomation.ViewModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MetroAutomation.Connection
{
    public class DeviceConnection : INotifyPropertyChanged
    {
        private LedState connectionState;
        private bool isConnected;

        public DeviceConnection(Device device)
        {
            Device = device;
            ConnectCommand = new AsyncCommandHandler(Connect);
            ConnectCommand = new AsyncCommandHandler(Disconnect);
            ToggleConnectionCommand = new AsyncCommandHandler(ToggleConnection);
        }

        public Device Device { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public IAsyncCommand ConnectCommand { get; set; }

        public IAsyncCommand DisconnectCommand { get; set; }

        public IAsyncCommand ToggleConnectionCommand { get; set; }

        public LedState ConnectionState
        {
            get
            {
                return connectionState;
            }
            private set
            {
                connectionState = value;
                OnPropertyChanged();
            }
        }

        public bool IsConnected
        {
            get
            {
                return isConnected;
            }
            private set
            {
                isConnected = value;
                OnPropertyChanged();
            }
        }

        public async Task Connect()
        {
            try
            {
                ConnectionState = LedState.Warn;
                await Device.Disconnect();
                await Device.Connect();
                IsConnected = true;
                ConnectionState = LedState.Success;
            }
            catch
            {
                ConnectionState = LedState.Fail;
            }
        }

        public async Task Disconnect()
        {
            ConnectionState = LedState.Warn;
            await Device.Disconnect();
            IsConnected = false;
            ConnectionState = LedState.Idle;
        }

        public async Task ToggleConnection()
        {
            if (IsConnected)
            {
                await Disconnect();
            }
            else
            {
                await Connect();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ConnectionManager
    {
        public ObservableCollection<DeviceConnection> Connections { get; } = new ObservableCollection<DeviceConnection>();

        public DeviceConnection LoadDevice(Device device)
        {
            DeviceConnection existing = ConnectionByConfigurationID(device.ConfigurationID);

            if (existing == null)
            {
                if (device.Configuration == null)
                {
                    device.Configuration = LiteDBAdaptor.LoadData<DeviceConfiguration>(device.ConfigurationID);
                }

                if (device.Configuration.CommandSet == null)
                {
                    device.Configuration.CommandSet = LiteDBAdaptor.LoadData<CommandSet>(device.Configuration.CommandSetID);
                }

                var connection = new DeviceConnection(device);
                Connections.Add(connection);

                return connection;
            }
            else
            {
                return existing;
            }
        }

        public DeviceConnection LoadDevice(int configurationID)
        {
            DeviceConnection existing = ConnectionByConfigurationID(configurationID);

            if (existing == null)
            {
                var configuration = LiteDBAdaptor.LoadData<DeviceConfiguration>(configurationID);
                configuration.CommandSet = LiteDBAdaptor.LoadData<CommandSet>(configuration.CommandSetID);
                return LoadDevice(new Device(configuration));
            }
            else
            {
                return existing;
            }
        }

        public void UnloadDevice(Device device)
        {
            var connection = Connections.FirstOrDefault(x => x.Device == device);

            if (connection != null)
            {
                Connections.Remove(connection);
            }
        }

        public void UnloadDevice(int configurationID)
        {
            var connection = ConnectionByConfigurationID(configurationID);

            if (connection != null)
            {
                Connections.Remove(connection);
            }
        }

        public DeviceConnection ConnectionByConfigurationID(int configurationID)
        {
            var connection = Connections.FirstOrDefault(x => x.Device.ConfigurationID == configurationID);
            return connection;
        }

        public void UpdateConfigurations(DeviceConfiguration configuration)
        {
            var devices = Connections
                .Where(x => x.Device.Configuration.ID == configuration.ID)
                .Select(x => x.Device)
                .ToArray();

            foreach (var device in devices)
            {
                device.Configuration = configuration;
            }
        }

        public void UpdateCommandSets(CommandSet commandSet)
        {
            var configurations = Connections
                .Where(x => x.Device.Configuration.CommandSetID == commandSet.ID)
                .Select(x => x.Device.Configuration)
                .ToArray();

            foreach (var configuration in configurations)
            {
                configuration.CommandSet = commandSet;
            }
        }


        public void LoadStandards()
        {
            
        }
    }
}
