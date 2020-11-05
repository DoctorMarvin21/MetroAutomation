using MetroAutomation.ExpressionEvaluation;
using MetroAutomation.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MetroAutomation.Calibration
{
    public enum Direction
    {
        Get,
        Set
    }

    public class Function : INotifyPropertyChanged
    {
        private RangeInfo rangeInfo;
        private ValueMultiplier valueMultiplier;
        private bool isValueErrorAvailable;

        protected Function(Device device, Mode mode, Direction direction)
        {
            Device = device;
            Mode = mode;
            Direction = direction;
            ProcessCommand = new AsyncCommandHandler(Process);
            ProcessBackgroundCommand = new AsyncCommandHandler(ProcessBackground);

            Components = FunctionDescription.GetComponents(this);
            Range = FunctionDescription.GetRange(this);
            Value = FunctionDescription.GetValue(this);
            ValueError = new ReadOnlyValueInfo(Value);

            MultipliedValue = new ReadOnlyValueInfo(Value);

            var modeInfo = Device.Configuration?.ModeInfo?.FirstOrDefault(x => x.Mode == mode);

            AvailableMultipliers = modeInfo?.Multipliers;
            AutoRange = modeInfo?.AutoRange ?? false;


            if (AvailableMultipliers?.Length > 0)
            {
                ValueMultiplier = AvailableMultipliers[0];
            }
            else
            {
                AvailableMultipliers = null;
            }

            UpdateMultipliedValue();

            foreach (var component in Components)
            {
                component.PropertyChanged += (s, e) => OnComponentsChanged();
            }

            Range.PropertyChanged += (s, e) => OnRangeChanged();
            Value.PropertyChanged += (s, e) => OnValueChanged();

            OnComponentsChanged();
            OnRangeChanged();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Device Device { get; }

        public Mode Mode { get; }

        public Direction Direction { get; }

        public ValueInfo Range { get; }

        public RangeInfo RangeInfo
        {
            get
            {
                return rangeInfo;
            }
            protected set
            {
                rangeInfo = value;
                OnRangeInfoChanged();
            }
        }

        public bool IsValueErrorAvailable
        {
            get
            {
                return isValueErrorAvailable;
            }
            private set
            {
                isValueErrorAvailable = value;
                OnPropertyChanged();
            }
        }

        public bool AutoRange { get; }

        public ValueInfo[] Components { get; }

        public ValueInfo Value { get; }

        public ValueMultiplier ValueMultiplier
        {
            get
            {
                return valueMultiplier;
            }
            set
            {
                valueMultiplier = value;
                OnPropertyChanged();
                UpdateMultipliedValue();
                UpdateValueError();
            }
        }

        public ReadOnlyValueInfo MultipliedValue { get; }

        public ReadOnlyValueInfo ValueError { get; }

        public ValueMultiplier[] AvailableMultipliers { get; }

        public IAsyncCommand ProcessCommand { get; }

        public IAsyncCommand ProcessBackgroundCommand { get; }

        public List<AttachedCommand> AttachedCommands { get; } = new List<AttachedCommand>();

        public async Task<bool> Process()
        {
            return await ProcessCommandHandler(false);
        }

        public async Task<bool> ProcessBackground()
        {
            return await ProcessCommandHandler(true);
        }

        protected virtual void OnRangeInfoChanged()
        {
            OnPropertyChanged(nameof(RangeInfo));
        }

        protected virtual void OnValueChanged()
        {
            OnPropertyChanged(nameof(Value));
            UpdateMultipliedValue();
            UpdateValueError();
        }

        protected virtual void ProcessResult(decimal? result, UnitModifier modifier)
        {
            if (RangeInfo?.Range.GetNormal() == 0)
            {
                var temp = new BaseValueInfo(result, Components[0].Unit, modifier);
                temp.AutoModifier();
                Components[0].FromValueInfo(temp, true);
            }
            else
            {
                Components[0].Value = ValueInfoUtils.UpdateModifier(result, modifier, Components[0].Modifier);
            }
        }

        protected virtual void OnRangeChanged()
        {
            RangeInfo = Utils.GetRange(this, Device.Configuration);

            if (Direction == Direction.Get)
            {
                Components[0].FromValueInfo(new BaseValueInfo(null, Range.Unit, Range.Modifier), true);
            }

            UpdateValueError();
        }

        protected virtual void OnComponentsChanged()
        {
            FunctionDescription.ComponentsToValue(this);

            RangeInfo = Utils.GetRange(this, Device.Configuration);

            if (Direction == Direction.Set)
            {
                foreach (var component in Components)
                {
                    component.OnRangeChanged();
                }
            }

            UpdateValueError();
        }

        protected virtual void UpdateValueError()
        {
            var evaluator = RangeInfo?.Evaluator;
            IsValueErrorAvailable = evaluator != null;

            if (IsValueErrorAvailable)
            {
                var parameters = evaluator.Parameters;
                var arguments = new double[parameters.Length];

                for (int i = 0; i < parameters.Length; i++)
                {
                    var argument = GetArgument(parameters[i]);

                    if (argument.HasValue)
                    {
                        arguments[i] = argument.Value;
                    }
                    else
                    {
                        BaseValueInfo temp = new BaseValueInfo(null, Value.Unit, Value.Modifier);
                        ValueError.FromValueInfo(temp, true);
                        return;
                    }
                }

                try
                {
                    double error = evaluator.EvaluateCompiled(arguments);

                    decimal? modified = ValueInfoUtils.UpdateModifier((decimal)error, UnitModifier.None, Value.Modifier);
                    BaseValueInfo temp = new BaseValueInfo(modified, Value.Unit, Value.Modifier);
                    ValueError.FromValueInfo(temp, true);
                }
                catch
                {
                    BaseValueInfo temp = new BaseValueInfo(null, Value.Unit, Value.Modifier);
                    ValueError.FromValueInfo(temp, true);
                }
            }
        }

        private double? GetArgument(string name)
        {
            if (name == "V")
            {
                return (double?)Value.GetNormal();
            }
            else if (name.StartsWith("V") && int.TryParse(name[1..^0], out int result))
            {
                if (result == 0)
                {
                    return (double?)Value.GetNormal();
                }
                else if (result - 1 < Components.Length)
                {
                    return (double?)Components[result - 1].GetNormal();
                }
                else
                {
                    return null;
                }
            }
            else if (name == "R")
            {
                return (double?)(RangeInfo?.Range.GetNormal() ?? Range.GetNormal());
            }
            else if (name == "N")
            {
                return (double?)ValueMultiplier?.Multiplier;
            }
            else
            {
                foreach (var attached in AttachedCommands)
                {
                    var value = attached.GetErrorArgumentValue(name);

                    if (value.HasValue)
                    {
                        return (double)value.Value;
                    }
                }

                return null;
            }
        }

        public void FromFunction(Function function)
        {
            Range.FromValueInfo(function.Range, true);

            for (int i = 0; i < Components.Length; i++)
            {
                ValueInfo component = Components[i];
                component.FromValueInfo(function.Components[i], true);
            }

            ValueMultiplier = function.ValueMultiplier;
        }

        protected virtual async Task<bool> ProcessCommandHandler(bool background)
        {
            foreach (var command in AttachedCommands)
            {
                if (command.AutoExecute == AutoExecuteType.BeforeMode)
                {
                    await command.Process(background);
                }
            }

            bool success;
            if (Direction == Direction.Set)
            {
                success = await Device.QueryAction(this, background);
            }
            else
            {
                var result = await Device.QueryResult(this, background);

                // Update value only if we can measure physically
                if (Device.Configuration.CommandSet.TryGetCommand(Mode, FunctionCommandType.Value, out _))
                {
                    ProcessResult(result, UnitModifier.None);
                }

                success = result.HasValue;
            }

            foreach (var command in AttachedCommands)
            {
                if (command.AutoExecute == AutoExecuteType.AfterValue)
                {
                    await command.Process(background);
                }
            }

            return success;
        }

        private void UpdateMultipliedValue()
        {
            BaseValueInfo temp = new BaseValueInfo(Value.Value * (ValueMultiplier?.Multiplier ?? 1), Value.Unit, Value.Modifier);
            MultipliedValue.FromValueInfo(temp, true);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static Function GetFunction(Device device, Mode mode)
        {
            switch (mode)
            {
                case Mode.SetACV:
                case Mode.SetDCV:
                case Mode.SetDCI:
                case Mode.SetACI:
                case Mode.SetRES2W:
                case Mode.SetRES4W:
                case Mode.SetCAP2W:
                case Mode.SetCAP4W:
                case Mode.SetIND2W:
                case Mode.SetIND4W:
                case Mode.SetADM4W:
                case Mode.SetDCP:
                case Mode.SetACP:
                case Mode.SetDCV_DCV:
                case Mode.SetACV_ACV:
                case Mode.SetTEMP:
                case Mode.SetFREQ:
                    {
                        return new Function(device, mode, Direction.Set);
                    }
                case Mode.GetACV:
                case Mode.GetDCV:
                case Mode.GetDCI:
                case Mode.GetACI:
                case Mode.GetRES2W:
                case Mode.GetRES4W:
                case Mode.GetCAP2W:
                case Mode.GetCAP4W:
                case Mode.GetIND2W:
                case Mode.GetIND4W:
                case Mode.GetADM4W:
                case Mode.GetDCP:
                case Mode.GetACP:
                case Mode.GetTEMP:
                case Mode.GetFREQ:
                    {
                        return new Function(device, mode, Direction.Get);
                    }
                default:
                    {
                        throw new NotImplementedException();
                    }
            }
        }
    }
}
