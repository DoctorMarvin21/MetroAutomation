using MetroAutomation.Calibration;
using MetroAutomation.ViewModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace MetroAutomation.FrontPanel
{
    public enum Fluke5520TcType
    {
        [Description("B-тип")]
        B,
        [Description("C-тип")]
        C,
        [Description("E-тип")]
        E,
        [Description("J-тип")]
        J,
        [Description("K-тип")] // Default
        K,
        [Description("N-тип")]
        N,
        [Description("R-тип")]
        R,
        [Description("S-тип")]
        S,
        [Description("T-тип")]
        T,
        [Description("10 мкВ/°C, линеный выход")]
        X,
        [Description("1 мV/°C, линеный выход")]
        Z,
        [Description("%, отновительная влажность")]
        Y
    }

    public enum Fluke5520RtdType
    {
        [Description("PT385 (100 Ом)")] // Default
        PT385,
        [Description("PT385 (200 Ом)")]
        PT385_200,
        [Description("PT385 (500 Ом)")]
        PT385_500,
        [Description("PT385 (1000 Ом)")]
        PT385_1000,
        [Description("PT3926 (100 Ом)")]
        PT3926,
        [Description("PT3916 (100 Ом)")]
        PT3916,
        [Description("CU10 (10 Ом)")]
        CU10,
        [Description("NI120 (120 Ом)")]
        NI120
    }

    public enum Fluke5520SensorType
    {
        [ExtendedDescription("Термопара", null, "Термопара")]
        TC,
        [ExtendedDescription("Термометр сопротивления", null, "Термометр сопротивления")]
        RTD
    }

    public class Fluke5520TermoResistance : AttachedCommand
    {
        private Fluke5520SensorType sensorType;
        private bool isTc;
        private bool isRtd;

        public Fluke5520TermoResistance(Function function)
            : base(function)
        {
            IsTc = SensorType == Fluke5520SensorType.TC;
            IsRtd = SensorType == Fluke5520SensorType.RTD;
        }

        public Fluke5520SensorType SensorType
        {
            get
            {
                return sensorType;
            }
            set
            {
                sensorType = value;
                OnPropertyChanged();

                IsTc = SensorType == Fluke5520SensorType.TC;
                IsRtd = SensorType == Fluke5520SensorType.RTD;
            }
        }

        public bool IsTc
        {
            get
            {
                return isTc;
            }
            private set
            {
                isTc = value;
                OnPropertyChanged();
            }
        }

        public bool IsRtd
        {
            get
            {
                return isRtd;
            }
            private set
            {
                isRtd = value;
                OnPropertyChanged();
            }
        }

        public Fluke5520TcType TcType { get; set; } = Fluke5520TcType.K;

        public Fluke5520RtdType RtdType { get; set; } = Fluke5520RtdType.PT385;

        public override AutoExecuteType AutoExecute => AutoExecuteType.AfterRange;

        public override async Task Process(bool background)
        {
            if (await Function.Device.QueryAction($"TSENS_TYPE {SensorType}; *OPC?", background))
            {
                if (IsTc)
                {
                    await Function.Device.QueryAction($"TC_TYPE {TcType}; *OPC?", background);
                }
                else
                {
                    await Function.Device.QueryAction($"RTD_TYPE {RtdType}; *OPC?", background);
                }
            }
        }
    }
}
