using LiteDB;
using System;
using System.Linq;

namespace MetroAutomation.Calibration
{
    public enum ValueInfoType
    {
        Range,
        Component,
        Value
    }

    [Serializable]
    public class ValueInfo : BaseValueInfo, IDiscreteValueInfo, IReadOnlyValueInfo
    {
        public ValueInfo(ValueInfoType type, Function function, decimal? value, Unit unit, UnitModifier modifier)
            : base(value, unit, modifier)
        {
            Type = type;
            Function = function;
            DiscreteValues = GetDiscreteValues();
            SetInitial();
        }

        public ValueInfo(ValueInfo source)
            : this(source.Type, source.Function, source.Value, source.Unit, source.Modifier)
        {
        }

        public ValueInfoType Type { get; }

        public bool IsReadOnly => Type == ValueInfoType.Value;

        public bool IsDiscrete => DiscreteValues?.Length > 0;

        public override bool HasErrors => !IsReadOnly && (TextInvalidFormat || NotInDiscrete() || Function.RangeInfo == null);

        public Function Function { get; }

        public ActualValueInfo[] DiscreteValues { get; }

        private ActualValueInfo[] GetDiscreteValues()
        {
            if (Type == ValueInfoType.Range)
            {
                return Function.Device.Configuration.ModeInfo?
                    .FirstOrDefault(x => x.Mode == Function.Mode)?
                    .Ranges?
                    .Select(x => x.Range)
                    .Where(x => x != null)
                    .Distinct()
                    .OrderBy(x => x.GetNormal())
                    .Select(x => new ActualValueInfo(x))
                    .ToArray();
            }
            else if (Type == ValueInfoType.Component)
            {
                var allowedUnits = FunctionDescription.Components[Function.Mode]
                    .Where(x => x.AllowedUnits.Contains(Unit))
                    .SelectMany(x => x.AllowedUnits)
                    .Distinct()
                    .ToArray();

                return Function.Device.Configuration.ModeInfo?
                    .FirstOrDefault(x => x.Mode == Function.Mode)?
                    .ActualValues?
                    .Where(x => x != null)
                    .OrderBy(x => x.Value.GetNormal())
                    .ToArray();
            }
            else
            {
                return null;
            }
        }

        public void OnRangeChanged()
        {
            OnErrorsChanged();
        }

        private void SetInitial()
        {
            if (Type == ValueInfoType.Range && Function.Direction == Direction.Get && DiscreteValues?.Length > 0)
            {
                FromValueInfo(DiscreteValues[0].Value, true);
            }
        }

        public override string ToString()
        {
            return TextValue;
        }
    }
}
