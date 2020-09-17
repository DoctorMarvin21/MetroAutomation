using MetroAutomation.Calibration;
using MetroAutomation.ViewModel;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace MetroAutomation.FrontPanel
{
    public enum ImpedanceMode
    {
        [ExtendedDescription("Cp-D", "", "Емкость (Cp) и затухание (D) в параллельном контуре")]
        CPD,
        [ExtendedDescription("Cp-Q", "", "Емкость (Cp) и добротность (Q) в параллельном контуре")]
        CPQ,
        [ExtendedDescription("Cp-G", "", "Емкость (Cp) и активная проводимость (G) в параллельном контуре")]
        CPG,
        [ExtendedDescription("Cp-Rp", "", "Емкость (Cp) и сопротивление (Rp) в параллельном контуре")]
        CPRP,
        [ExtendedDescription("Cs-D", "", "Емкость (Cs) и затухание (D) в последовательном контуре")]
        CSD,
        [ExtendedDescription("Cs-Q", "", "Емкость (Cs) и добротность (Q) в последовательном контуре")]
        CSQ,
        [ExtendedDescription("Cs-Rs", "", "Емкость (Cs) и сопротивление (Rs) в последовательном контуре")]
        CSRS,
        [ExtendedDescription("Lp-D", "", "Индуктивность (Lp) и затухание (D) в параллельном контуре")]
        LPD,
        [ExtendedDescription("Lp-Q", "", "Индуктивность (Lp) и добротность (Q) в параллельном контуре")]
        LPQ,
        [ExtendedDescription("Lp-G", "", "Индуктивность (Lp) и активная проводимость (G) в параллельном контуре")]
        LPG,
        [ExtendedDescription("Lp-Rp", "", "Индуктивность (Lp) и сопротивление (Rp) в параллельном контуре")]
        LPRP,
        [ExtendedDescription("Ls-D", "", "Индуктивность (Ls) и затухание (D) в последовательном контуре")]
        LSD,
        [ExtendedDescription("Ls-Q", "", "Индуктивность (Ls) и добротность (Q) в последовательном контуре")]
        LSQ,
        [ExtendedDescription("Ls-Rs", "", "Индуктивность (Ls) и сопротивление (Rs) в последовательном контуре")]
        LSRS,
        [ExtendedDescription("R-X", "", "Активное (R) и реактивное (X) сопротивление")]
        RX,
        [ExtendedDescription("Z-θd", "", "Полное сопротивление (Z) и фазовый угол в градусах (θd)")]
        ZTD,
        [ExtendedDescription("Z-θr", "", "Полное сопротивление (Z) и фазовый угол в радианах (θr)")]
        ZTR,
        [ExtendedDescription("G-B", "", "Активная (G) и реактивная (B) проводимости")]
        GB,
        [ExtendedDescription("Y-θd", "", "Полная проводимость (Y) и фазовый угол в градусах (θd)")]
        YTD,
        [ExtendedDescription("Y-θr", "", "Полная проводимость (Y) и фазовый угол в радианах (θr)")]
        YTR
    }

    public class ImpedanceMeasurement : AttachedCommand
    {
        private string additionalValueName;

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

        public string AdditionalValueName
        {
            get
            {
                return additionalValueName;
            }
            private set
            {
                additionalValueName = value;
                OnPropertyChanged();
            }
        }

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
                    case Mode.GetIND4W:
                        {
                            MainValue.Unit = Unit.H;
                            AllowedModes = new ImpedanceMode[] { ImpedanceMode.LPD, ImpedanceMode.LPQ, ImpedanceMode.LPG, ImpedanceMode.LPRP, ImpedanceMode.LSD, ImpedanceMode.LSQ, ImpedanceMode.LSRS };
                            SelectedMode = ImpedanceMode.LPD;
                            break;
                        }
                    case Mode.GetADM4W:
                        {
                            MainValue.Unit = Unit.S;
                            AllowedModes = new ImpedanceMode[] { ImpedanceMode.YTD, ImpedanceMode.YTR };
                            SelectedMode = ImpedanceMode.YTD;
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

                AdditionalValue.Unit = ImpedanceHelper.SecondUnitInfo[selectedMode].Item1;
                AdditionalValueName = ImpedanceHelper.SecondUnitInfo[selectedMode].Item2;

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

            if (normalAverages < 1)
            {
                Averages.Value = 1;
            }
            else if (normalAverages > 256)
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
                AdditionalValue.Value = null;

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

    public static class ImpedanceHelper
    {
        public static Dictionary<ImpedanceMode, (Unit, string)> SecondUnitInfo { get; } = new Dictionary<ImpedanceMode, (Unit, string)>
        {
            { ImpedanceMode.CPD, (Unit.None, "Затухание") },
            { ImpedanceMode.CPQ, (Unit.None, "Добротность") },
            { ImpedanceMode.CPG, (Unit.S, "Aктивная проводимость") },
            { ImpedanceMode.CPRP, (Unit.Ohm, "Сопротивление") },
            { ImpedanceMode.CSD, (Unit.None, "Затухание") },
            { ImpedanceMode.CSQ, (Unit.None, "Добротность") },
            { ImpedanceMode.CSRS, (Unit.Ohm, "Сопротивление") },
            { ImpedanceMode.LPD, (Unit.None, "Затухание") },
            { ImpedanceMode.LPQ, (Unit.None, "Добротность") },
            { ImpedanceMode.LPG, (Unit.S, "Aктивная проводимость") },
            { ImpedanceMode.LPRP, (Unit.Ohm, "Сопротивление") },
            { ImpedanceMode.LSD, (Unit.None, "Затухание") },
            { ImpedanceMode.LSQ, (Unit.None, "Добротность") },
            { ImpedanceMode.LSRS, (Unit.Ohm, "Сопротивление") },
            { ImpedanceMode.RX, (Unit.Ohm, "Сопротивление") },
            { ImpedanceMode.ZTD, (Unit.DA, "Фазовый угол") },
            { ImpedanceMode.ZTR, (Unit.RA, "Фазовый угол") },
            { ImpedanceMode.GB, (Unit.S, "Реактивная проводимость") },
            { ImpedanceMode.YTD, (Unit.DA, "Фазовый угол") },
            { ImpedanceMode.YTR, (Unit.RA, "Фазовый угол") }
        };
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

            if (device.Functions.TryGetValue(Mode.GetIND4W, out Function inductance))
            {
                inductance.AttachedCommands.Add(ImpedanceMeasurement);
            }

            if (device.Functions.TryGetValue(Mode.GetADM4W, out Function admittance))
            {
                admittance.AttachedCommands.Add(ImpedanceMeasurement);
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
