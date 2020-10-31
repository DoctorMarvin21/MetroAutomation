using MetroAutomation.Calibration;
using System.Linq;

namespace MetroAutomation.Automation
{
    public interface IInitializableValueInfo : IValueInfo
    {
        bool IsInitialized { get; }

        void Initialize();
    }

    public class MultiplierValueInfo : BaseValueInfo, IDiscreteValueInfo, IInitializableValueInfo
    {
        private bool isInitialized;

        public MultiplierValueInfo(Function function)
        {
            Function = function;
            DiscreteValues = function.AvailableMultipliers?
                .Select(x => new ActualValueInfo(new BaseValueInfo(x.Multiplier, Unit.None, UnitModifier.None), x.Name))
                .ToArray();
        }

        public Function Function { get; }

        public bool IsInitialized => isInitialized;

        public bool IsDiscrete => true;

        public override decimal? Value
        {
            get
            {
                return Function.ValueMultiplier?.Multiplier ?? 1;
            }
            set
            {
                SetMultiplier(value);

                OnPropertyChanged();
                UpdateText();
            }
        }

        public override string TextValue
        {
            get => Function.ValueMultiplier?.Name;
            set { }
        }

        public ActualValueInfo[] DiscreteValues { get; }

        public override void FromValueInfo(IValueInfo valueInfo, bool updateText)
        {
            SetMultiplier(valueInfo.Value);
            OnPropertyChanged(string.Empty);
        }

        private void SetMultiplier(decimal? value)
        {
            var selected = Function.AvailableMultipliers?.FirstOrDefault(x => x.Multiplier == value);

            if (selected == null && Function.AvailableMultipliers?.Length > 0)
            {
                selected = Function.AvailableMultipliers[0];
            }

            Function.ValueMultiplier = selected;
        }

        public void Initialize()
        {
            if (!IsInitialized)
            {
                isInitialized = true;
            }
        }
    }
}
