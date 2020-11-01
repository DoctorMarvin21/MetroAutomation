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

        private readonly ConnectionSettings connectionSettingsCopy;

        public DeviceConnection(Device device)
        {
            Device = device;
            connectionSettingsCopy = device.ConnectionSettings.BinaryDeepClone();
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

        private void ConnectionChanged(object sender, DeviceConnectionChangedEventArgs e)
        {
            bool oldIsConnected = IsConnected;

            IsConnected = e.IsConnected;

            if (IsConnected && !oldIsConnected && !connectionSettingsCopy.DeepBinaryEquals(Device.ConnectionSettings))
            {
                // Saving connection settings on success
                LiteDBAdaptor.SaveData(Device.Configuration);
            }

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

    public class ConnectionManager : INotifyPropertyChanged
    {
        private readonly object loadLockers = new object();
        private string lastError;

        public ConnectionManager(MainViewModel owner)
        {
            Owner = owner;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<DeviceConnection> Connections { get; } = new ObservableCollection<DeviceConnection>();

        public BindableCollection<DeviceLogEventArgs> Logs { get; } = new BindableCollection<DeviceLogEventArgs>();

        public MainViewModel Owner { get; }

        public string LastError
        {
            get
            {
                return lastError;
            }
            private set
            {
                lastError = value;
                OnPropertyChanged();
            }
        }

        public DeviceConnection LoadDevice(int configurationID)
        {
            lock (loadLockers)
            {
                DeviceConnection existing = ConnectionByConfigurationID(configurationID);

                if (existing == null)
                {
                    var configuration = LiteDBAdaptor.LoadData<DeviceConfiguration>(configurationID);
                    configuration.CommandSet = LiteDBAdaptor.LoadData<CommandSet>(configuration.CommandSetID);
                    var device = new Device(configuration);

                    var connection = new DeviceConnection(device);

                    // skipping dummy connection
                    if (device.ConfigurationID != 0)
                    {
                        Connections.Add(connection);
                        device.Log += DeviceLog;
                    }

                    return connection;
                }
                else
                {
                    return existing;
                }
            }
        }

        public void UnloadDevice(Device device)
        {
            UnloadDevice(device.ConfigurationID);
        }

        public void UnloadDevice(int configurationID)
        {
            lock (loadLockers)
            {
                var connection = ConnectionByConfigurationID(configurationID);

                if (connection != null)
                {
                    connection.Device.Log -= DeviceLog;
                    Connections.Remove(connection);
                    connection.Device.Dispose();
                }
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

        private void DeviceLog(object sender, DeviceLogEventArgs e)
        {
            if (!e.IsSuccess)
            {
                LastError = $"{e.Device.Configuration.Name}: \"{e.Text}\"";
            }

            Logs.Add(e);
        }

        public async Task DisconnectAndUnloadAllDevices()
        {
            if (Connections.Count == 0)
            {
                return;
            }

            for (int i = 0; i < Connections.Count; i++)
            {
                DeviceConnection connection = Connections[i];
                await connection.Disconnect();
                connection.Device.Log -= DeviceLog;

                connection.Device.Dispose();
            }

            Connections.Clear();
        }

        public void UnloadUnusedDisconnectedDevices()
        {
            var usedConnections = GetUsedConnections();
            var fixedConnections = Connections.ToArray();

            foreach (var connection in fixedConnections)
            {
                if (!usedConnections.Contains(connection) && !connection.IsConnected)
                {
                    UnloadDevice(connection.Device);
                }
            }
        }

        public async Task DisconnectAndUnloadUnusedDevices()
        {
            var usedConnections = GetUsedConnections();
            var fixedConnections = Connections.ToArray();

            foreach (var connection in fixedConnections)
            {
                if (!usedConnections.Contains(connection))
                {
                    await connection.Device.Disconnect();
                    UnloadDevice(connection.Device);
                }
            }
        }

        private DeviceConnection[] GetUsedConnections()
        {
            return (Owner.ProtocolManager?.DeviceProtocol?.GetUsedConnections() ?? new DeviceConnection[0])
                .Union(Owner.FrontPanelManager?.GetUsedConnections() ?? new DeviceConnection[0])
                .Distinct()
                .ToArray();
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
