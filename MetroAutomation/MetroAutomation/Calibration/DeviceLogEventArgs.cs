using System;

namespace MetroAutomation.Calibration
{
    public enum DeviceLogEntryType
    {
        Connected,
        Disconnected,
        DataSend,
        DataReceived
    }

    public class DeviceLogEventArgs : EventArgs
    {
        public DeviceLogEventArgs(Device device, string text, DeviceLogEntryType type)
        {
            Device = device;
            Text = text;
            Type = type;
        }

        public Device Device { get; }

        public string Text { get; }

        public DeviceLogEntryType Type { get; }
    }
}
