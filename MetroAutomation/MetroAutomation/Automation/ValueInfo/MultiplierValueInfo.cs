using MetroAutomation.Calibration;
using System.Linq;

namespace MetroAutomation.Automation
{
    public class MultiplierValueInfo : BaseValueInfo, IDiscreteValueInfo
    {
        public MultiplierValueInfo(Function function)
        {
            Function = function;
            FromValueInfo(function.ValueMultiplier?.Value, true);
            DiscreteValues = function.AvailableMultipliers?
                .Select(x => new ActualValueInfo(new BaseValueInfo(x.Value), x.Name))
                .ToArray();
        }

        public Function Function { get; }

        public bool IsDiscrete => true;

        public override string TextValue
        {
            get => Function.ValueMultiplier?.Name;
            set { }
        }

        public ActualValueInfo[] DiscreteValues { get; }

        public override void FromValueInfo(IValueInfo valueInfo, bool updateText)
        {
            base.FromValueInfo(valueInfo, updateText);
            SetMultiplier();
        }

        private void SetMultiplier()
        {
            Function.ValueMultiplier = Function.AvailableMultipliers?.FirstOrDefault(x => x?.Value.Equals(this) ?? false);
        }
    }
}
