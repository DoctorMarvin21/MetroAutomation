using MetroAutomation.Calibration;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MetroAutomation.FrontPanel
{
    public class TransmilleSimulatedModeCommand : AttachedCommand
    {
        private readonly NumberFormatInfo numberFormat = new NumberFormatInfo
        {
            NumberDecimalSeparator = "."
        };

        private string tempRangeCommand;

        private bool isSimulated;

        public TransmilleSimulatedModeCommand(Function function)
            : base(function)
        {
            Simulated = new BaseValueInfo(function.Value);
        }

        public override AutoExecuteType AutoExecute => AutoExecuteType.BeforeMode;

        public bool IsSimulated
        {
            get
            {
                return isSimulated;
            }
            set
            {
                isSimulated = value;
                OnPropertyChanged();
            }
        }

        public BaseValueInfo Simulated { get; }

        public override decimal? GetErrorArgumentValue(string argument)
        {
            return null;
        }

        public override async Task Process(bool background)
        {
            var temp = Function.Device.Configuration.CommandSet.FunctionCommands.FirstOrDefault(x => x?.Mode == Function.Mode);

            if (IsSimulated)
            {
                if (temp?.RangeCommand != null)
                {
                    tempRangeCommand = temp.RangeCommand;
                    temp.RangeCommand = null;
                }

                if (Function.Mode == Mode.SetRES2W)
                {
                    await Function.Device.QueryAction(GetResistanceCommandByValue(Simulated.GetNormal()), background);
                }
            }
            else
            {
                if (temp != null && tempRangeCommand != null)
                {
                    Function.Device.ResetRangeAndMode();
                    temp.RangeCommand = tempRangeCommand;
                }
            }
        }

        private string GetResistanceCommandByValue(decimal? value)
        {
            string command;

            if (value >= 10 && value < 100)
            {
                command = $"I2/R27/O{ConvertValueToString(value, UnitModifier.None)}";
            }
            else if (value < 1000)
            {
                command = $"I2/R28/O{ConvertValueToString(value, UnitModifier.Kilo)}";
            }
            else if (value < 10000)
            {
                command = $"I2/R29/O{ConvertValueToString(value, UnitModifier.Kilo)}";
            }
            else if (value < 100000)
            {
                command = $"I2/R30/O{ConvertValueToString(value, UnitModifier.Kilo)}";
            }
            else if (value < 1000000)
            {
                command = $"I2/R31/O{ConvertValueToString(value, UnitModifier.Mega)}";
            }
            else if (value <= 10000000)
            {
                command = $"I2/R32/O{ConvertValueToString(value, UnitModifier.Mega)}";
            }
            else
            {
                command = $"I2";
            }

            return command;
        }

        private string ConvertValueToString(decimal? value, UnitModifier unitModifier)
        {
            var modified = ValueInfoUtils.UpdateModifier(value, UnitModifier.None, unitModifier);

            if (modified.HasValue)
            {
                return modified.Value.ToString(numberFormat);
            }
            else
            {
                return string.Empty;
            }
        }

        public override void Reset()
        {
            IsSimulated = false;
        }
    }
}
