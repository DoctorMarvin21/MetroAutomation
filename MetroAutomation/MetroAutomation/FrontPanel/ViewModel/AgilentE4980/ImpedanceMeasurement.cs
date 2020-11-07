using MetroAutomation.Calibration;
using System.Globalization;
using System.Threading.Tasks;

namespace MetroAutomation.FrontPanel
{
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

        public ImpedanceMeasurement(FrontPanelViewModel frontPanel)
            : base(null)
        {
            FrontPanel = frontPanel;

            Frequency = new BaseValueInfo(1, Unit.Hz, UnitModifier.Kilo);
            Voltage = new BaseValueInfo(1, Unit.V, UnitModifier.None);
            Averages = new BaseValueInfo(1, Unit.None, UnitModifier.None);

            MainValue = new BaseValueInfo(null, Unit.F, UnitModifier.None);
            AdditionalValue = new BaseValueInfo(null, Unit.None, UnitModifier.None);
        }

        public FrontPanelViewModel FrontPanel { get; }

        public override AutoExecuteType AutoExecute => AutoExecuteType.AfterValue;

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

                _ = Process(false);
            }
        }

        public override decimal? GetErrorArgumentValue(string argument)
        {
            switch (argument)
            {
                case "FREQ":
                    {
                        return Frequency.GetNormal();
                    }
                case "VOLT":
                    {
                        return Voltage.GetNormal();
                    }
                case "AVG":
                    {
                        return Averages.GetNormal();
                    }
                case "SUB":
                    {
                        return AdditionalValue.GetNormal();
                    }
                default:
                    {
                        return null;
                    }
            }
        }

        public override async Task Process(bool background)
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

            if (normalFrequency.HasValue && lastFrequency != normalFrequency && await FrontPanel.Device.QueryAction(Function, $":FREQ {normalFrequency.Value.ToString(CultureInfo.InvariantCulture)};*OPC?", background))
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

            if (normalVoltage.HasValue && lastVoltage != normalVoltage && await FrontPanel.Device.QueryAction(Function, $":VOLT {normalVoltage.Value.ToString(CultureInfo.InvariantCulture)};*OPC?", background))
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

            if (normalAveragesInteger.HasValue && lastAverages != normalAveragesInteger && await FrontPanel.Device.QueryAction(Function, $":APER LONG, {normalAveragesInteger.Value};*OPC?", background))
            {
                lastAverages = normalAveragesInteger;
            }


            if (lastMode != SelectedMode && await FrontPanel.Device.QueryAction(Function, $":FUNC:IMP {SelectedMode};*OPC?", background))
            {
                lastMode = SelectedMode;
            }

            string measurementData = await FrontPanel.Device.QueryAsync(":FETCh?", background);

            if (measurementData != null)
            {
                string[] values = measurementData.Split(',');

                if (values.Length > 0 && decimal.TryParse(values[0], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal mainValue))
                {
                    var temp = new BaseValueInfo(mainValue, MainValue.Unit, UnitModifier.None);
                    temp.AutoModifier();
                    MainValue.FromValueInfo(temp, true);

                    CurrentFunction?.Components[0].FromValueInfo(MainValue, true);
                }
                else
                {
                    MainValue.Value = null;

                    if (CurrentFunction != null)
                    {
                        CurrentFunction.Components[0].Value = null;
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
                    CurrentFunction.Components[0].Value = null;
                }
            }
        }

        public override void Reset()
        {
            lastMode = null;
            lastFrequency = null;
            lastVoltage = null;
            lastAverages = null;
        }
    }
}
