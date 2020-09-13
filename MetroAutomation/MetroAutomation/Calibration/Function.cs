using MetroAutomation.ViewModel;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MetroAutomation.Calibration
{
    public enum Mode
    {
        [ExtendedDescription("DCV", "Напряжение постоянного тока", "Измерение напряжения постоянного тока")]
        GetDCV,
        [ExtendedDescription("DCV", "Напряжение постоянного тока", "Установка напряжения постоянного тока")]
        SetDCV,
        [ExtendedDescription("ACV", "Напряжение переменного тока", "Измерение напряжения переменного тока")]
        GetACV,
        [ExtendedDescription("ACV", "Напряжение переменного тока", "Установка напряжения переменного тока")]
        SetACV,
        [ExtendedDescription("DCI", "Сила постоянного тока", "Измерение силы постоянного тока")]
        GetDCI,
        [ExtendedDescription("DCI", "Сила постоянного тока", "Установка силы постоянного тока")]
        SetDCI,
        [ExtendedDescription("ACI", "Сила переменного тока", "Измерение силы переменного тока")]
        GetACI,
        [ExtendedDescription("ACI", "Сила переменного тока", "Установка силы переменного тока")]
        SetACI,
        [ExtendedDescription("RES2W", "Сопротивление по двухпроводной схеме", "Измерение сопротивления по двухпроводной схеме")]
        GetRES2W,
        [ExtendedDescription("RES2W", "Сопротивление по двухпроводной схеме", "Установка сопротивления по двухпроводной схеме")]
        SetRES2W,
        [ExtendedDescription("RES4W", "Сопротивление по четырехпроводной схеме", "Измерение сопротивления по четырехпроводной схеме")]
        GetRES4W,
        [ExtendedDescription("RES4W", "Сопротивление по четырехпроводной схеме", "Установка сопротивления по четырехпроводной схеме")]
        SetRES4W,
        [ExtendedDescription("CAP2W", "Емкость по двухпроводной схеме", "Измерение емкости по двухпроводной схеме")]
        GetCAP2W,
        [ExtendedDescription("CAP2W", "Емкость по двухпроводной схеме", "Установка емкости по двухпроводной схеме")]
        SetCAP2W,
        [ExtendedDescription("CAP4W", "Емкость по четырехпроводной схеме", "Измерение емкости по четырехпроводной схеме")]
        GetCAP4W,
        [ExtendedDescription("CAP4W", "Емкость по четырехпроводной схеме", "Установка емкости по четырехпроводной схеме")]
        SetCAP4W,
        [ExtendedDescription("DCP", "Мощность постоянного тока", "Измерение мощности постоянного тока")]
        GetDCP,
        [ExtendedDescription("DCP", "Мощность постоянного тока", "Установка мощности постоянного тока")]
        SetDCP,
        [ExtendedDescription("ACP", "Мощность переменного тока", "Измерение мощности переменного тока")]
        GetACP,
        [ExtendedDescription("ACP", "Мощность переменного тока", "Установка мощности переменного тока")]
        SetACP
    }

    public class Function
    {
        private RangeInfo rangeInfo;
        private ValueMultiplier currentMultiplier;

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
                CurrentMultiplier = AvailableMultipliers[0];
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

        public ValueMultiplier CurrentMultiplier
        {
            get
            {
                return currentMultiplier;
            }
            set
            {
                currentMultiplier = value;
                Value.Multiplier = currentMultiplier?.Multiplier;
            }
        }

        public ValueMultiplier[] AvailableMultipliers { get; }

        public IAsyncCommand ProcessCommand { get; }

        public IAsyncCommand ProcessBackgroundCommand { get; }

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
            Components[0].Value = Utils.UpdateModifier(result, modifiler, Components[0].Modifier);
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
            if (Direction == Direction.Set)
            {
                return await Device.QueryAction(this, background);
            }
            else
            {
                var result = await Device.QueryResult(this, background);
                ProcessResult(result, UnitModifier.None);

                return result.HasValue;
            }
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
