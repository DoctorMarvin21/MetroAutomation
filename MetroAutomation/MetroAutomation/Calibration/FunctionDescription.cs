using System;
using System.Collections.Generic;
using System.Linq;

namespace MetroAutomation.Calibration
{
    public static class FunctionDescription
    {
        public static Dictionary<Mode, ComponentDescription[]> Components { get; }

        public static Dictionary<Mode, ComponentDescription> Values { get; }

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
                            DefaultValue = new BaseValueInfo(null, Unit.Ohm, UnitModifier.Kilo),
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
                            DefaultValue = new BaseValueInfo(null, Unit.Ohm, UnitModifier.Kilo),
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
                {
                    Mode.GetCAP2W,
                    new[]
                    {
                        new ComponentDescription
                        {
                            ShortName = "C",
                            FullName = "Емкость",
                            DefaultValue = new BaseValueInfo(null, Unit.F, UnitModifier.Micro),
                            AllowedUnits = new[] { Unit.F }
                        }
                    }
                },
                {
                    Mode.SetCAP2W,
                    new[]
                    {
                        new ComponentDescription
                        {
                            ShortName = "C",
                            FullName = "Емкость",
                            DefaultValue = new BaseValueInfo(1, Unit.F, UnitModifier.Micro),
                            AllowedUnits = new[] { Unit.F }
                        }
                    }
                },
                {
                    Mode.GetCAP4W,
                    new[]
                    {
                        new ComponentDescription
                        {
                            ShortName = "C",
                            FullName = "Емкость",
                            DefaultValue = new BaseValueInfo(null, Unit.F, UnitModifier.Micro),
                            AllowedUnits = new[] { Unit.F }
                        }
                    }
                },
                {
                    Mode.SetCAP4W,
                    new[]
                    {
                        new ComponentDescription
                        {
                            ShortName = "C",
                            FullName = "Емкость",
                            DefaultValue = new BaseValueInfo(1, Unit.F, UnitModifier.Micro),
                            AllowedUnits = new[] { Unit.F }
                        }
                    }
                },
                {
                    Mode.GetDCP,
                    new[]
                    {
                        new ComponentDescription
                        {
                            ShortName = "P",
                            FullName = "Мощность",
                            DefaultValue = new BaseValueInfo(1, Unit.W, UnitModifier.None),
                            AllowedUnits = new[] { Unit.W }
                        }
                    }
                },
                {
                    Mode.SetDCP,
                    new[]
                    {
                        new ComponentDescription
                        {
                            ShortName = "V",
                            FullName = "Напряжение",
                            DefaultValue = new BaseValueInfo(1, Unit.V, UnitModifier.None),
                            AllowedUnits = new[] { Unit.V }
                        },
                        new ComponentDescription
                        {
                            ShortName = "A",
                            FullName = "Сила тока",
                            DefaultValue = new BaseValueInfo(1, Unit.A, UnitModifier.Mili),
                            AllowedUnits = new[] { Unit.A }
                        },
                        new ComponentDescription
                        {
                            ShortName = "°",
                            FullName = "Сдвиг фазы",
                            DefaultValue = new BaseValueInfo(0, Unit.CP, UnitModifier.None),
                            AllowedUnits = new[] { Unit.CP, Unit.KP }
                        }
                    }
                },
            };

            Values = new Dictionary<Mode, ComponentDescription>();
            Ranges = new Dictionary<Mode, ComponentDescription>();

            foreach (var component in Components)
            {
                switch (component.Key)
                {
                    case Mode.SetDCP:
                        {
                            Ranges.Add(component.Key, 
                            new ComponentDescription
                            {
                                ShortName = "P",
                                FullName = "Мощность",
                                DefaultValue = new BaseValueInfo(null, Unit.W, UnitModifier.None),
                                AllowedUnits = new[] { Unit.W }
                            });

                            Values.Add(component.Key,
                            new ComponentDescription
                            {
                                ShortName = "P",
                                FullName = "Мощность",
                                DefaultValue = new BaseValueInfo(null, Unit.W, UnitModifier.None),
                                AllowedUnits = new[] { Unit.W }
                            });
                            break;
                        }
                    default:
                        {
                            Ranges.Add(component.Key, component.Value[0]);
                            Values.Add(component.Key, component.Value[0]);
                            break;
                        }
                }
            }
        }

        public static ValueInfo[] GetComponents(Function function)
        {
            return Components[function.Mode].Select(x => new ValueInfo(ValueInfoType.Component, function, x.DefaultValue.Value, x.DefaultValue.Unit, x.DefaultValue.Modifier)).ToArray();
        }

        public static ValueInfo GetValue(Function function)
        {
            var defaultValue = Values[function.Mode].DefaultValue;
            return new ValueInfo(ValueInfoType.Value, function, defaultValue.Value, defaultValue.Unit, defaultValue.Modifier);
        }

        public static RangeInfo GetDefaultRangeInfo(Mode mode)
        {
            return new RangeInfo
            {
                Output = "Default",
                Range = new BaseValueInfo(null, Values[mode].DefaultValue.Unit, Values[mode].DefaultValue.Modifier),
                ComponentsRanges = Components[mode].Select(x =>
                    new ValueRange(
                        new BaseValueInfo(null, x.DefaultValue.Unit, x.DefaultValue.Modifier),
                        new BaseValueInfo(null, x.DefaultValue.Unit, x.DefaultValue.Modifier)))
                .ToArray()
            };
        }

        public static ActualValueInfo GetDefaultActualValue(Mode mode)
        {
            return new ActualValueInfo(Values[mode].DefaultValue);
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
                        int index = Array.FindIndex(valueInfo.Function.Components, x => ReferenceEquals(x, valueInfo));
                        return Components[valueInfo.Function.Mode][index];
                    }
                case ValueInfoType.Value:
                    {
                        return Values[valueInfo.Function.Mode];
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

        public static decimal? UnitConverter(decimal? value, Unit unit, Unit desiredUnit)
        {
            switch (unit)
            {
                case Unit.CP:
                    {
                        switch (desiredUnit)
                        {
                            case Unit.CP:
                                {
                                    return value;
                                }
                            case Unit.KP:
                                {
                                    if (value.HasValue)
                                    {
                                        return (decimal)Math.Cos((double)value * Math.PI / 180d);
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                            default:
                                {
                                    throw new NotImplementedException();
                                }
                        }
                    }
                case Unit.KP:
                    {
                        switch (desiredUnit)
                        {
                            case Unit.CP:
                                {
                                    if (value.HasValue)
                                    {
                                        return (Math.Acos((double)value) * 180d / Math.PI).ToDecimalSafe();
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                            case Unit.KP:
                                {
                                    return value;
                                }
                            default:
                                {
                                    throw new NotImplementedException();
                                }
                        }
                    }
                default:
                    {
                        return value;
                    }
            }
            
        }

        public static void ComponentsToValue(Function function)
        {
            switch (function.Mode)
            {
                case Mode.SetDCP:
                    {
                        decimal? p = function.Components[0].GetNormal() * function.Components[1].GetNormal();

                        if (function.Components[2].Unit == Unit.CP)
                        {
                            var angle = (double)(function.Components[2].GetNormal() ?? 0) * Math.PI / 180d;
                            p *= Math.Cos(angle).ToDecimalSafe();
                        }
                        else
                        {
                            p *= function.Components[2].GetNormal();
                        }

                        p = p.Normalize();

                        function.Value.FromValueInfo(new BaseValueInfo(p, Unit.W, UnitModifier.None), true);

                        break;
                    }
                default:
                    {
                        if (function.Components[0].IsDiscrete)
                        {
                            var discreteValue = function.Components[0]
                                .DiscreteValues
                                .FirstOrDefault(x => x.Value.AreValuesEqual(function.Components[0]));

                            if (discreteValue != null)
                            {
                                BaseValueInfo baseValueInfo = new BaseValueInfo(discreteValue.ActualValue);
                                baseValueInfo.UpdateModifier(function.Components[0].Modifier);

                                function.Value.FromValueInfo(baseValueInfo, true);
                            }
                            else
                            {
                                function.Value.FromValueInfo(function.Components[0], true);
                            }
                        }
                        else
                        {
                            function.Value.FromValueInfo(function.Components[0], true);
                        }
                        break;
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
