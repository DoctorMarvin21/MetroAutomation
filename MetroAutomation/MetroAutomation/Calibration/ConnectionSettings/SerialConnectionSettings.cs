using System;
using System.ComponentModel;

namespace MetroAutomation.Calibration
{
    public enum SerialBaudRate
    {
        [Description("75")]
        X75 = 75,
        [Description("110")]
        X110 = 110,
        [Description("300")]
        X300 = 300,
        [Description("1200")]
        X1200 = 1200,
        [Description("2400")]
        X2400 = 2400,
        [Description("4800")]
        X4800 = 4800,
        [Description("9600")]
        X9600 = 9600,
        [Description("19200")]
        X19200 = 19200,
        [Description("38400")]
        X38400 = 38400,
        [Description("57600")]
        X57600 = 57600,
        [Description("115200")]
        X115200 = 115200
    }

    public enum SerialDataBits : short
    {
        [Description("5")]
        Five = 5,
        [Description("6")]
        Six = 6,
        [Description("7")]
        Seven = 7,
        [Description("8")]
        Eight = 8
    }

    public enum SerialFlowControl
    {
        [Description("Нет")]
        None = 0,
        [Description("XON/XOFF")]
        XOnXOff = 1,
        [Description("RTS/CTS")]
        RtsCts = 2,
        [Description("RTS/CTS+XON/XOFF")]
        RtsCtsXOnXOff = 3,
        [Description("DTR/DSR")]
        DtrDsr = 4,
        [Description("DTR/DSR+XON/XOFF")]
        DtrDsrXOnXOff = 5
    }

    public enum SerialParity
    {
        [Description("Нет")]
        None = 0,
        [Description("Odd")]
        Odd = 1,
        [Description("Even")]
        Even = 2,
        [Description("Mark")]
        Mark = 3,
        [Description("Space")]
        Space = 4
    }

    public enum SerialStopBits
    {
        [Description("1")]
        One = 10,
        [Description("1,5")]
        OneAndHalf = 15,
        [Description("2")]
        Two = 20
    }

    [Serializable]
    public class SerialConnectionSettings : AdvancedConnectionSettings
    {
        public override ConnectionType Type => ConnectionType.Serial;

        public SerialBaudRate BaudRate { get; set; } = SerialBaudRate.X9600;

        public SerialDataBits DataBits { get; set; } = SerialDataBits.Eight;

        public SerialFlowControl FlowControl { get; set; } = SerialFlowControl.None;

        public SerialParity Parity { get; set; } = SerialParity.None;

        public SerialStopBits StopBits { get; set; } = SerialStopBits.One;

        public bool DtrEnable { get; set; }

        public bool RtsEnable { get; set; }

        public override void FromConnectionString(string connectionString)
        {
            string[] split = connectionString.Split(ConnectionUtils.Splitter);

            if (split.Length > 0 && int.TryParse(split[0].Replace(ConnectionUtils.Tags[Type], string.Empty), out int boardIndex))
            {
                BoardIndex = boardIndex;
            }
            else
            {
                BoardIndex = null;
            }
        }

        public override string ToConnectionString()
        {
            return $"{ConnectionUtils.Tags[Type]}{BoardIndex}{ConnectionUtils.Splitter}{ConnectionUtils.InstrumentTag}";
        }
    }
}