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
            MultipliedValue = new BaseValueInfo(Value);

            AvailableMultipliers = Device.Configuration?.ModeInfo?.FirstOrDefault(x => x.Mode == mode)?.Multipliers;
            AutoRange = Device.Configuration?.ModeInfo?.FirstOrDefault(x => x.Mode == mode)?.AutoRange ?? false;

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
            }
        }

        public BaseValueInfo MultipliedValue { get; }

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
            if (!AutoRange)
            {
                RangeInfo = Utils.GetRange(this, Device.Configuration);
            }

            if (Direction == Direction.Get)
            {
                Components[0].Value = null;
                Components[0].Modifier = Range.Modifier;
                Components[0].Unit = Range.Unit;
            }
        }

        protected virtual void OnComponentsChanged()
        {
            FunctionDescription.ComponentsToValue(this);

            if (!AutoRange && Direction == Direction.Set)
            {
                RangeInfo = Utils.GetRange(this, Device.Configuration);

                foreach (var component in Components)
                {
                    component.OnRangeChanged();
                }
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
