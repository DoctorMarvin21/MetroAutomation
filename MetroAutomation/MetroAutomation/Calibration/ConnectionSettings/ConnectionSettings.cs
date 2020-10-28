using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MetroAutomation.Calibration
{
    public enum ConnectionType
    {
        [Description("Ручное управление")]
        Manual,
        [Description("Последовательный порт")]
        Serial,
        [Description("GPIB")]
        Gpib,
        [Description("Prologix GPIB")]
        GpibPrologix,
        [Description("USB")]
        Usb,
    }

    public enum Termination
    {
        [Description("-")]
        None,
        [Description("LF")]
        Lf,
        [Description("CR")]
        Cr,
        [Description("CR+LF")]
        Crlf
    }

    [Serializable]
    public class ConnectionSettings : INotifyPropertyChanged
    {
        private ConnectionType type;
        private AdvancedConnectionSettings advancedConnectionSettings = new ManualConnectionSettings();

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public int Timeout { get; set; } = 30000;

        public int PauseAfterRead { get; set; } = 50;

        public int PauseAfterWrite { get; set; } = 50;

        public Termination Termination { get; set; } = Termination.Lf;

        public AdvancedConnectionSettings AdvancedConnectionSettings
        {
            get
            {
                return advancedConnectionSettings;
            }
            set
            {
                advancedConnectionSettings = value;
                OnPropertyChanged();

                if (AdvancedConnectionSettings != null && Type != advancedConnectionSettings.Type)
                {
                    type = advancedConnectionSettings.Type;
                    OnPropertyChanged(nameof(Type));
                }
            }
        }

        public ConnectionType Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;

                OnPropertyChanged();

                if (AdvancedConnectionSettings?.Type != type)
                {
                    advancedConnectionSettings = ConnectionUtils.GetConnectionSettingsByType(type);
                    OnPropertyChanged(nameof(AdvancedConnectionSettings));
                }
            }
        }

        public string GetNewLineString()
        {
            switch (Termination)
            {
                case Termination.None:
                    {
                        return string.Empty;
                    }
                case Termination.Cr:
                    {
                        return "\r";
                    }
                case Termination.Lf:
                    {
                        return "\n";
                    }
                case Termination.Crlf:
                    {
                        return "\r\n";
                    }
                default:
                    {
                        throw new ArgumentException();
                    }
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}