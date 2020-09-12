using System;
using System.Collections.Generic;
using System.Linq;

namespace MetroAutomation.Calibration
{
    public static class FunctionDescription
    {
        public static Dictionary<Mode, ComponentDescription[]> Components { get; }

        public static Dictionary<Mode, ComponentDescription[]> Values { get; }

        public static Dictionary<Mode, ComponentDescription> Ranges { get; }

        static FunctionDescription()
        {
            Components = new Dictionary<Mode, ComponentDescription[]>
            {
                {
                    Mode.GetDCV,
                    new[]
                    {
                        new ComponentDescription
                        {
                            ShortName = "U",
                            FullName = "Напряжение",
                            DefaultValue = new BaseValueInfo(null, Unit.V, UnitModifier.None),
                            AllowedUnits = new[] { Unit.V }
                        }
                    }
                },
                {
                    Mode.SetDCV,
                    new[]
                    {
                        new ComponentDescription
                        {
                            ShortName = "U",
                            FullName = "Напряжение",
                            DefaultValue = new BaseValueInfo(1, Unit.V, UnitModifier.None),
                            AllowedUnits = new[] { Unit.V }
                        }
                    }
                },
                {
                    Mode.GetACV,
                    new[]
                    {
                        new ComponentDescription
                        {
                            ShortName = "U",
                            FullName = "Напряжение",
                            DefaultValue = new BaseValueInfo(null, Unit.V, UnitModifier.None),
                            AllowedUnits = new[] { Unit.V }
                        }
                    }
                },
                {
                    Mode.SetACV,
                    new[]
                    {
                        new ComponentDescription
                        {
                            ShortName = "U",
                            FullName = "Напряжение",
                            DefaultValue = new BaseValueInfo(1, Unit.V, UnitModifier.None),
                            AllowedUnits = new[] { Unit.V }
                        },
                        new ComponentDescription
                        {
                            ShortName = "F",
                            FullName = "Частота",
                            DefaultValue = new BaseValueInfo(1, Unit.Hz, UnitModifier.Kilo),
                            AllowedUnits = new[] { Unit.Hz }
                        }
                    }
                },
                {
                    Mode.GetDCI,
                    new[]
                    {
                        new ComponentDescription
                        {
                            ShortName = "F",
                            FullName = "Сила тока",
                            DefaultValue = new BaseValueInfo(null, Unit.A, UnitModifier.Mili),
                            AllowedUnits = new[] { Unit.A }
                        }
                    }
                },
                {
                    Mode.SetDCI,
                    new[]
                    {
                        new ComponentDescription
                        {
                            ShortName = "F",
                            FullName = "Сила тока",
                            DefaultValue = new BaseValueInfo(1, Unit.A, UnitModifier.Mili),
                            AllowedUnits = new[] { Unit.A }
                        }
                    }
                },
                {
                    Mode.GetACI,
                    new[]
                    {
                        new ComponentDescription
                        {
                            ShortName = "F",
                            FullName = "Сила тока",
                            DefaultValue = new BaseValueInfo(null, Unit.A, UnitModifier.Mili),
                            AllowedUnits = new[] { Unit.A }
                        }
                    }
                },
                {
                    Mode.SetACI,
                    new[]
                    {
                        new ComponentDescription
                        {
                            ShortName = "F",
                            FullName = "Сила тока",
                            DefaultValue = new BaseValueInfo(1, Unit.A, UnitModifier.Mili),
                            AllowedUnits = new[] { Unit.A }
                        },
                        new ComponentDescription
                        {
                            ShortName = "F",
                            FullName = "Частота",
                            DefaultValue = new BaseValueInfo(1, Unit.Hz, UnitModifier.Kilo),
                            AllowedUnits = new[] { Unit.Hz }
                        }
                    }
                },
                {
                    Mode.GetRES2W,
                    new[]
                    {
                        new ComponentDescription
                        {
                            ShortName = "R",
                            FullName = "Сопротивление",
                            DefaultValue = new BaseValueInfo(1, Unit.Ohm, UnitModifier.Kilo),
                            AllowedUnits = new[] { Unit.Ohm }
                        }
                    }
                },
                {
                    Mode.SetRES2W,
                    new[]
                    {
                        new ComponentDescription
                        {
                            ShortName = "R",
                            FullName = "Сопротивление",
                            DefaultValue = new BaseValueInfo(1, Unit.Ohm, UnitModifier.Kilo),
                            AllowedUnits = new[] { Unit.Ohm }
                        }
                    }
                },
                {
                    Mode.GetRES4W,
                    new[]
                    {
                        new ComponentDescription
                        {
                            ShortName = "R",
                            FullName = "Сопротивление",
                            DefaultValue = new BaseValueInfo(1, Unit.Ohm, UnitModifier.Kilo),
                            AllowedUnits = new[] { Unit.Ohm }
                        }
                    }
                },
                {
                    Mode.SetRES4W,
                    new[]
                    {
                        new ComponentDescription
                        {
                            ShortName = "R",
                            FullName = "Сопротивление",
                            DefaultValue = new BaseValueInfo(1, Unit.Ohm, UnitModifier.Kilo),
                            AllowedUnits = new[] { Unit.Ohm }
                        }
                    }
                },
            };

            Values = new Dictionary<Mode, ComponentDescription[]>();
            Ranges = new Dictionary<Mode, ComponentDescription>();

            foreach (var component in Components)
            {
                switch (component.Key)
                {
                    // TODO: Add another components
                    default:
                        {
                            Ranges.Add(component.Key, component.Value[0]);
                            Values.Add(component.Key, new ComponentDescription[] { component.Value[0] });
                            break;
                        }
                }
            }
        }

        public static ValueInfo[] GetComponents(Function function)
        {
            return Components[function.Mode].Select(x => new ValueInfo(ValueInfoType.Component, function, x.DefaultValue.Value, x.DefaultValue.Unit, x.DefaultValue.Modifier)).ToArray();
        }

        public static ValueInfo[] GetValues(Function function)
        {
            return Values[function.Mode].Select(x => new ValueInfo(ValueInfoType.Value, function, x.DefaultValue.Value, x.DefaultValue.Unit, x.DefaultValue.Modifier)).ToArray();
        }

        public static RangeInfo GetDefaultRangeInfo(Mode mode)
        {
            return new RangeInfo
            {
                Output = "Default",
                Range = new BaseValueInfo(null, Values[mode][0].DefaultValue.Unit, Values[mode][0].DefaultValue.Modifier),
                ComponentsRanges = Components[mode].Select(x =>
                    new ValueRange(
                        new BaseValueInfo(null, x.DefaultValue.Unit, x.DefaultValue.Modifier),
                        new BaseValueInfo(null, x.DefaultValue.Unit, x.DefaultValue.Modifier)))
                .ToArray()
            };
        }

        public static ActualValueInfo GetDefaultActualValue(Mode mode)
        {
            return new ActualValueInfo(Values[mode][0].DefaultValue);
        }

        public static ValueInfo GetRange(Function function)
        {
            var description = Ranges[function.Mode];
            return new ValueInfo(ValueInfoType.Range, function, null, description.DefaultValue.Unit, description.DefaultValue.Modifier);
        }

        public static ComponentDescription GetDescription(ValueInfo valueInfo)
        {
            switch (valueInfo.Type)
            {
                case ValueInfoType.Component:
                    {
                        int index = Array.IndexOf(valueInfo.Function.Components, valueInfo);
                        return Components[valueInfo.Function.Mode][index];
                    }
                case ValueInfoType.Value:
                    {
                        int index = Array.IndexOf(valueInfo.Function.Values, valueInfo);
                        return Values[valueInfo.Function.Mode][index];
                    }
                case ValueInfoType.Range:
                    {
                        return Ranges[valueInfo.Function.Mode];
                    }
                default:
                    {
                        throw new NotImplementedException();
                    }
            }
        }
    }

    public class ComponentDescription
    {
        public string ShortName { get; set; }

        public string FullName { get; set; }

        public BaseValueInfo DefaultValue { get; set; }

        public Unit[] AllowedUnits { get; set; }
    }
}
