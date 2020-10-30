using System;
using System.ComponentModel;

namespace MetroAutomation.Calibration
{
    public enum DeviceLogEntryType
    {
        [Description("Подключен")]
        Connected,
        [Description("Ошибка соединения")]
        ConnectError,
        [Description("Отключен")]
        Disconnected,
        [Description("Запись")]
        DataSend,
        [Description("Чтение")]
        DataReceived,
        [Description("Ошибка запроса")]
        QueryError
    }

    public class DeviceLogEventArgs : EventArgs
    {
        public DeviceLogEventArgs(Device device, bool isSuccess, string text, DeviceLogEntryType type)
        {
            Timestamp = DateTime.Now;
            Device = device;
            IsSuccess = isSuccess;
            Text = text;
            Type = type;
        }

        public DateTime Timestamp { get; }

        public Device Device { get; }

        public bool IsSuccess { get; }

        public string Text { get; }

        public DeviceLogEntryType Type { get; }
    }
}
