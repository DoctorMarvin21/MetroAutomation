using System;

namespace MetroAutomation.Calibration
{
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
