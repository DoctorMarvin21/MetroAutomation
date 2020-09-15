using System;
using System.ComponentModel;

namespace MetroAutomation.Calibration
{
    public enum ConnectionStatus
    {
        [Description("Идёт подключение...")]
        Connecting,
        [Description("Подключен")]
        Connected,
        [Description("Ошибка соединения")]
        ConnectError,
        [Description("Идёт отключение...")]
        Disconnecting,
        [Description("Отключен")]
        Disconnected,
        [Description("Потеря соединения")]
        ConnectionLost
    }

    public class DeviceConnectionChangedEventArgs : EventArgs
    {
        public DeviceConnectionChangedEventArgs(Device device, bool isConnected, ConnectionStatus status)
        {
            Device = device;
            IsConnected = isConnected;
            Status = status;
        }

        public Device Device { get; }

        public bool IsConnected { get; }

        public ConnectionStatus Status { get; }
    }
}
