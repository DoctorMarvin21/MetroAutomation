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
        Gpib
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

                if (AdvancedConnectionSettings?.Type != type)
                {
                    switch (type)
                    {
                        case ConnectionType.Manual:
                            {
                                AdvancedConnectionSettings = new ManualConnectionSettings();
                                break;
                            }
                        case ConnectionType.Serial:
                            {
                                AdvancedConnectionSettings = new SerialConnectionSettings();
                                break;
                            }
                        case ConnectionType.Gpib:
                            {
                                AdvancedConnectionSettings = new GpibConnectionSettings();
                                break;
                            }
                    }
                }

                OnPropertyChanged();
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

    [Serializable]
    public abstract class AdvancedConnectionSettings
    {
        public abstract ConnectionType Type { get; }

        public abstract string GenerateConnectionString();
    }

    [Serializable]
    public class ManualConnectionSettings : AdvancedConnectionSettings
    {
        public override ConnectionType Type => ConnectionType.Manual;

        public override string GenerateConnectionString()
        {
            return string.Empty;
        }
    }

    [Serializable]
    public class SerialConnectionSettings : AdvancedConnectionSettings
    {
        public override ConnectionType Type => ConnectionType.Serial;

        public override string GenerateConnectionString()
        {
            return string.Empty;
        }
    }

    [Serializable]
    public class GpibConnectionSettings : AdvancedConnectionSettings
    {
        public override ConnectionType Type => ConnectionType.Gpib;

        public int AdapterIndex { get; set; }

        public int DeviceIndex { get; set; }

        public override string GenerateConnectionString()
        {
            return $"GPIB{AdapterIndex}::{DeviceIndex}::INSTR";
        }
    }
}