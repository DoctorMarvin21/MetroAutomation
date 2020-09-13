using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MetroAutomation.Calibration
{
    public static class ValueInfoUtils
    {
        private static readonly Dictionary<string, Unit> units;
        private static readonly Dictionary<string, UnitModifier> modifiers;

        private static readonly Dictionary<string, (Unit, UnitModifier)> unitInfo;

        static ValueInfoUtils()
        {
            units = EnumExtensions.GetValues<Unit>().ToDictionary(x => x.GetDescription(), x => x);
            modifiers = EnumExtensions.GetValues<UnitModifier>().ToDictionary(x => x.GetDescription(), x => x);

            unitInfo = new Dictionary<string, (Unit, UnitModifier)>();

            foreach (var unit in units)
            {
                foreach (var modifier in modifiers)
                {
                    unitInfo.Add($"{modifier.Key}{unit.Key}", (unit.Value, modifier.Value));
                }
            }
        }

        public static string GetTextValue(IValueInfo valueInfo)
        {
            return $"{(valueInfo.Value * (valueInfo.Multiplier ?? 1))?.ToString() ?? "-"} {valueInfo.Modifier.GetDescription()}{valueInfo.Unit.GetDescription()}";
        }

        public static (string, Unit, UnitModifier)[] GetUnits(Unit[] units)
        {
            return units.Select(x => GetAllowedModifiers(x)
                .Select(y => ($"{y.GetDescription()}{x.GetDescription()}", x, y))).SelectMany(x => x).ToArray();
        }

        public static UnitModifier[] GetAllowedModifiers(Unit unit)
        {
            switch (unit)
            {
                case Unit.CP:
                case Unit.KP:
                    {
                        return new[] { UnitModifier.None };
                    }
                default:
                    {
                        return EnumExtensions.GetValues<UnitModifier>().OrderBy(x => (int)x).ToArray();
                    }
            }
        }

        public static BaseValueInfo FromTextValue(string text, IValueInfo valueInfo)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            StringBuilder numericPart = new StringBuilder();
            StringBuilder unitPart = new StringBuilder();

            bool numericFilled = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (numericFilled)
                {
                    if (!char.IsWhiteSpace(c))
                    {
                        unitPart.Append(c);
                    }
                }
                else
                {
                    if (char.IsDigit(c) || c == '.' || c == ',' || c == '-')
                    {
                        if (c == ',')
                        {
                            numericPart.Append('.');
                        }
                        else
                        {
                            numericPart.Append(c);
                        }
                    }
                    else if (!char.IsWhiteSpace(c))
                    {
                        numericFilled = true;
                        unitPart.Append(c);
                    }
                }
            }

            if (decimal.TryParse(numericPart.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal value))
            {
                string unitString = unitPart.ToString();

                if (unitPart.Length == 0)
                {
                    return new BaseValueInfo(value, valueInfo.Unit, valueInfo.Modifier);
                }
                else
                {
                    if (modifiers.TryGetValue(unitString, out UnitModifier modifier))
                    {
                        return new BaseValueInfo(value, valueInfo.Unit, modifier);
                    }
                    else if (unitInfo.TryGetValue(unitPart.ToString(), out var unit))
                    {
                        if (valueInfo is ValueInfo fullValueInfo)
                        {
                            if (FunctionDescription.GetDescription(fullValueInfo).AllowedUnits.Contains(unit.Item1))
                            {
                                return new BaseValueInfo(value, unit.Item1, unit.Item2);
                            }
                            else
                            {
                                return null;
                            }
                        }
                        else
                        {
                            return new BaseValueInfo(value, unit.Item1, unit.Item2);
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else
            {
                return null;
            }
        }
    }
}
