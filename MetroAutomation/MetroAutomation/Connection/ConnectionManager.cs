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
        private string connectionText;
        private LedState connectionState;
        private bool isConnected;

        public DeviceConnection(Device device)
        {
            ConnectionText = ConnectionStatus.Disconnected.GetDescription();

            Device = device;
            device.ConnectionChanged += ConnectionChanged;
            ConnectCommand = new AsyncCommandHandler(Connect);
            ConnectCommand = new AsyncCommandHandler(Disconnect);
            ToggleConnectionCommand = new AsyncCommandHandler(ToggleConnection);
        }

        public Device Device { get; }

        public string ConnectionText
        {
            get
            {
                return connectionText;
            }
            private set
            {
                connectionText = value;
                OnPropertyChanged();
            }
        }

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
            await Device.Disconnect();
            await Device.Connect();
        }

        public async Task Disconnect()
        {
            await Device.Disconnect();
        }

        public async Task ToggleConnection()
        {
            if (Device.IsConnected)
            {
                await Disconnect();
            }
            else
            {
                await Connect();
            }
        }

        private void ConnectionChanged(object sender, ConnectionChangedEventArgs e)
        {
            IsConnected = e.IsConnected;

            switch (e.Status)
            {
                case ConnectionStatus.Connecting:
                case ConnectionStatus.Disconnecting:
                    {
                        ConnectionState = LedState.Warn;
                        break;
                    }
                case ConnectionStatus.ConnectError:
                case ConnectionStatus.ConnectionLost:
                    {
                        ConnectionState = LedState.Fail;
                        break;
                    }
                case ConnectionStatus.Disconnected:
                    {
                        ConnectionState = LedState.Idle;
                        break;
                    }
                case ConnectionStatus.Connected:
                    {
                        ConnectionState = LedState.Success;
                        break;
                    }
            }

            ConnectionText = e.Status.GetDescription();
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
