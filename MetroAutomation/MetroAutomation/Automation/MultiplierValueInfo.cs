using MetroAutomation.Calibration;
using System.Linq;

namespace MetroAutomation.Automation
{
    public class MultiplierValueInfo : BaseValueInfo, IDiscreteValueInfo
    {
        public MultiplierValueInfo(Function function)
        {
            Function = function;
            DiscreteValues = function.AvailableMultipliers?
                .Select(x => new ActualValueInfo(new BaseValueInfo(x.Multiplier, Unit.None, UnitModifier.None), x.Name))
                .ToArray();
        }

        public Function Function { get; }

        public bool IsDiscrete => true;

        public override decimal? Value
        {
            get
            {
                return Function.ValueMultiplier?.Multiplier;
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
            Function.ValueMultiplier = selected;
        }
    }
}
