using System;
using System.ComponentModel;

namespace MetroAutomation.Calibration
{
    public enum DeviceLogEntryType
    {
        [Description("Подключен")]
        Connected,
        [Description("Отключен")]
        Disconnected,
        [Description("Запись")]
        DataSend,
        [Description("Чтение")]
        DataReceived
    }

    public class DeviceLogEventArgs : EventArgs
    {
        public DeviceLogEventArgs(Device device, string text, DeviceLogEntryType type)
        {
            Timestamp = DateTime.Now;
            Device = device;
            Text = text;
            Type = type;
        }

        public DateTime Timestamp { get; }

        public Device Device { get; }

        public string Text { get; }

        public DeviceLogEntryType Type { get; }
    }
}
