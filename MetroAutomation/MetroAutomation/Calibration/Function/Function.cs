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

            AvailableMultipliers = Device.Configuration?.ModeInfo?.FirstOrDefault(x => x.Mode == mode)?.Multipliers;

            if (AvailableMultipliers?.Length > 0)
            {
                ValueMultiplier = AvailableMultipliers[0];
            }
            else
            {
                AvailableMultipliers = null;
            }

            foreach (var component in Components)
            {
                component.PropertyChanged += (s, e) => OnComponentsChanged();
            }

            Range.PropertyChanged += (s, e) => OnRangeChanged();

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
            }
        }

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
        }

        protected virtual void ProcessResult(decimal? result, UnitModifier modifiler)
        {
            Components[0].Value = ValueInfoUtils.UpdateModifier(result, modifiler, Components[0].Modifier);
        }

        protected virtual void OnRangeChanged()
        {
            RangeInfo = Utils.GetRange(this, Device.Configuration);

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

            if (Direction == Direction.Set)
            {
                RangeInfo = Utils.GetRange(this, Device.Configuration);
            }
        }

        protected virtual async Task<bool> ProcessCommandHandler(bool background)
        {
            bool success;
            if (Direction == Direction.Set)
            {
                success = await Device.QueryAction(this, background);
            }
            else
            {
                var result = await Device.QueryResult(this, background);
                ProcessResult(result, UnitModifier.None);

                success = result.HasValue;
            }

            if (success)
            {
                var attached = AttachedCommands.ToArray();

                foreach (var command in attached)
                {
                    if (command.AutoExecute)
                    {
                        await command.Process();
                    }
                }
            }

            return success;
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
                case Mode.SetDCP:
                case Mode.SetACP:
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
                case Mode.GetDCP:
                case Mode.GetACP:
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
