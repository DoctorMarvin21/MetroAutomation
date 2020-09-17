using MetroAutomation.Calibration;
using MetroAutomation.ViewModel;
using System.Globalization;
using System.Threading.Tasks;

namespace MetroAutomation.FrontPanel
{
    public enum ImpedanceMode
    {
        [ExtendedDescription("Cp-D", "", "")]
        CPD,
        CPQ,
        CPG,
        CPRP,
        CSD,
        CSQ,
        CSRS,
        LPD,
        LPQ,
        LPG,
        LPRP,
        LPRD,
        LSD,
        LSQ,
        LSRS,
        LSRD,
        RX,
        ZTD,
        ZTR,
        GB,
        YTD,
        YTR,
        VDID
    }

    public class ImpedanceMeasurement : AttachedCommand
    {
        private Function currentFunction;
        private ImpedanceMode[] allowedModes;
        private ImpedanceMode selectedMode;

        private ImpedanceMode? lastMode;
        public decimal? lastFrequency;
        public decimal? lastVoltage;
        public int? lastAverages;

        public ImpedanceMeasurement(FrontPanelViewModel frontPanel, Device device)
            : base(device)
        {
            AutoExecute = true;
            FrontPanel = frontPanel;

            Frequency = new BaseValueInfo(1, Unit.Hz, UnitModifier.Kilo);
            Voltage = new BaseValueInfo(1, Unit.V, UnitModifier.None);
            Averages = new BaseValueInfo(1, Unit.None, UnitModifier.None);

            MainValue = new BaseValueInfo(null, Unit.F, UnitModifier.None);
            AdditionalValue = new BaseValueInfo(null, Unit.None, UnitModifier.None);
        }

        public FrontPanelViewModel FrontPanel { get; }

        public BaseValueInfo MainValue { get; }

        public BaseValueInfo Frequency { get; }

        public BaseValueInfo Voltage { get; }

        public BaseValueInfo Averages { get; }

        public BaseValueInfo AdditionalValue { get; }

        public Function CurrentFunction
        {
            get
            {
                return currentFunction;
            }
            set
            {
                currentFunction = value;

                switch (currentFunction?.Mode)
                {
                    case Mode.GetCAP4W:
                        {
                            MainValue.Unit = Unit.F;
                            AllowedModes = new ImpedanceMode[] { ImpedanceMode.CPD, ImpedanceMode.CPQ, ImpedanceMode.CPG, ImpedanceMode.CPRP, ImpedanceMode.CSD, ImpedanceMode.CSQ, ImpedanceMode.CSRS };
                            SelectedMode = ImpedanceMode.CPD;
                            break;
                        }
                    case Mode.GetRES4W:
                        {
                            MainValue.Unit = Unit.Ohm;
                            AllowedModes = new ImpedanceMode[] { ImpedanceMode.RX, ImpedanceMode.ZTD, ImpedanceMode.ZTR };
                            SelectedMode = ImpedanceMode.RX;
                            break;
                        }
                }
            }
        }

        public ImpedanceMode[] AllowedModes
        {
            get
            {
                return allowedModes;
            }
            private set
            {
                allowedModes = value;
                OnPropertyChanged();
            }
        }

        public ImpedanceMode SelectedMode
        {
            get
            {
                return selectedMode;
            }
            set
            {
                selectedMode = value;
                OnPropertyChanged();
                _ = Process();
            }
        }

        public override async Task Process()
        {
            var normalFrequency = Frequency.GetNormal();

            if (normalFrequency < 20)
            {
                normalFrequency = 20;

                Frequency.Value = 20;
                Frequency.Modifier = UnitModifier.None;
            }
            else if (normalFrequency > 2000000)
            {
                normalFrequency = 2000000;

                Frequency.Value = 2;
                Frequency.Modifier = UnitModifier.Mega;
            }

            if (normalFrequency.HasValue && lastFrequency != normalFrequency && await Device.QueryAction($":FREQ {normalFrequency.Value.ToString(CultureInfo.InvariantCulture)};*OPC?", false))
            {
                lastFrequency = normalFrequency;
            }

            var normalVoltage = Voltage.GetNormal();

            if (normalVoltage > 2)
            {
                normalVoltage = 2;

                Voltage.Value = 2;
                Voltage.Modifier = UnitModifier.None;
            }

            if (normalVoltage.HasValue && lastVoltage != normalVoltage && await Device.QueryAction($":VOLT {normalVoltage.Value.ToString(CultureInfo.InvariantCulture)};*OPC?", false))
            {
                lastVoltage = normalVoltage;
            }

            var normalAverages = Averages.GetNormal();
            var normalAveragesInteger = (int?)normalAverages;

            if (normalAverages > normalAveragesInteger)
            {
                Averages.Value = 256;
            }
            else if (normalAverages != normalAveragesInteger)
            {
                Averages.Value = normalAveragesInteger;
            }

            if (normalAveragesInteger.HasValue && lastAverages != normalAveragesInteger && await Device.QueryAction($":APER LONG, {normalAveragesInteger.Value};*OPC?", false))
            {
                lastAverages = normalAveragesInteger;
            }


            if (lastMode != SelectedMode && await Device.QueryAction($":FUNC:IMP {SelectedMode};*OPC?", false))
            {
                lastMode = SelectedMode;
            }

            string measurementData = await Device.QueryAsync(":FETCh?", FrontPanel.IsInfiniteReading);

            if (measurementData != null)
            {
                string[] values = measurementData.Split(',');

                if (values.Length > 0 && decimal.TryParse(values[0], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal mainValue))
                {
                    var temp = new BaseValueInfo(mainValue, MainValue.Unit, UnitModifier.None);
                    temp.AutoModifier();
                    MainValue.FromValueInfo(temp, true);

                    CurrentFunction?.Value.FromValueInfo(MainValue, true);
                }
                else
                {
                    MainValue.Value = null;

                    if (CurrentFunction != null)
                    {
                        CurrentFunction.Value.Value = null;
                    }
                }

                if (values.Length > 1 && decimal.TryParse(values[1], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal additionalValue))
                {
                    var temp = new BaseValueInfo(additionalValue, AdditionalValue.Unit, UnitModifier.None);
                    temp.AutoModifier();
                    AdditionalValue.FromValueInfo(temp, true);
                }
                else
                {
                    AdditionalValue.Value = null;
                }
            }
            else
            {
                MainValue.Value = null;

                if (CurrentFunction != null)
                {
                    CurrentFunction.Value.Value = null;
                }
            }
        }

        public void Reset()
        {
            lastMode = null;
            lastFrequency = null;
            lastVoltage = null;
            lastAverages = null;
        }
    }

    public class AgilentE4980AFrontPanelViewModel : FrontPanelViewModel
    {
        public AgilentE4980AFrontPanelViewModel(Device device)
            : base(device)
        {
            ImpedanceMeasurement = new ImpedanceMeasurement(this, device)
            {
                CurrentFunction = SelectedFunction
            };

            if (device.Functions.TryGetValue(Mode.GetCAP4W, out Function capacitance))
            {
                capacitance.AttachedCommands.Add(ImpedanceMeasurement);
            }

            if (device.Functions.TryGetValue(Mode.GetRES4W, out Function resistance))
            {
                resistance.AttachedCommands.Add(ImpedanceMeasurement);
            }
        }

        public override FrontPanelType Type => FrontPanelType.Agilent4980A;

        public ImpedanceMeasurement ImpedanceMeasurement { get; }

        protected override Task OnFunctionChanged(Function oldFunction, Function newFunction)
        {
            if (ImpedanceMeasurement != null)
            {
                ImpedanceMeasurement.CurrentFunction = newFunction;
            }

            return base.OnFunctionChanged(oldFunction, newFunction);
        }

        protected override Task OnConnectionChangedChanged(bool isConnected)
        {
            ImpedanceMeasurement.Reset();
            return base.OnConnectionChangedChanged(isConnected);
        }
    }
}
