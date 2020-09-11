using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MetroAutomation.Calibration
{
    internal static class Utils
    {
        public static RangeInfo GetRange(Function function, DeviceConfiguration configuration)
        {
            if (configuration.TryGetRanges(function.Mode, out var ranges) && ranges.Length > 0)
            {
                if (function.Range.Value.HasValue)
                {
                    RangeInfo range = ranges.FirstOrDefault(x => AreValuesEqual(x.Range, function.Range));

                    if (range == null)
                    {
                        range = new RangeInfo { Range = new BaseValueInfo(function.Range), Output = "Default" };
                    }

                    return range;
                }
                else
                {
                    foreach (RangeInfo range in ranges)
                    {
                        bool allTrue = true;

                        for (int i = 0; i < range.ComponentsRanges.Length; i++)
                        {
                            allTrue = range.ComponentsRanges[i].FitsRange(function.Components[i]);

                            if (!allTrue)
                            {
                                break;
                            }
                        }

                        if (allTrue)
                        {
                            return range;
                        }
                    }

                    return null;
                }
            }
            else
            {
                return new RangeInfo { Range = new BaseValueInfo(function.Range.Value ?? 0, function.Range.Unit, function.Range.Modifier), Output = "Default" };
            }
        }

        public static string FillCommand(string command, Function function, DeviceConfiguration configuration)
        {
            // <Value(range)+index;UnitMode;unit separator;decimal separator>
            // unit modes: N - value + unit + modifier (default), V - value only, U - unit only
            // "<V;1;U;.>"

            int current = 0;
            StringBuilder sb = new StringBuilder();

            while (current >= 0)
            {
                int indexStart = command.IndexOf('<', current);

                if (indexStart >= 0)
                {
                    int indexEnd = command.IndexOf('>', current);

                    if (indexEnd >= 0)
                    {
                        sb.Append(command[current..indexStart]);

                        string arg = command.Substring(indexStart + 1, indexEnd - indexStart - 1);
                        sb.Append(GetValue(arg, function, configuration));

                        current = indexEnd + 1;
                    }
                    else
                    {
                        current = indexEnd;
                    }
                }
                else
                {
                    sb.Append(command[current..]);
                    current = indexStart;
                }
            }

            return sb.ToString();
        }

        private static string GetValue(string arg, Function function, DeviceConfiguration configuration)
        {
            string[] argData = arg.Split(';');

            string paramType = argData[0];
            int paramIndex = int.Parse(argData[1]) - 1;

            string paramFormat;

            if (argData.Length > 2)
            {
                paramFormat = argData[2];
            }
            else
            {
                paramFormat = "N";
            }

            string unitSeparator;

            if (argData.Length > 3)
            {
                unitSeparator = argData[3];
            }
            else
            {
                unitSeparator = " ";
            }

            string decimalSeparator;

            if (argData.Length > 4)
            {
                decimalSeparator = argData[4];
            }
            else
            {
                decimalSeparator = ".";
            }

            IValueInfo value;

            switch (paramType)
            {
                case "V":
                    {
                        value = function.Components[paramIndex];
                        break;
                    }
                case "R":
                    {
                        if (function.Range.Value.HasValue)
                        {
                            value = function.Range;
                        }
                        else
                        {
                            value = function.RangeInfo.Range;
                        }

                        break;
                    }
                default:
                    {
                        throw new InvalidOperationException($"Unknown parameter type: \"{paramType}\".");
                    }
            }

            NumberFormatInfo numberFormat = new NumberFormatInfo
            {
                NumberDecimalSeparator = decimalSeparator
            };

            string textValue;

            if (paramFormat != "N")
            {
                textValue = (value.GetNormal() ?? 0).Normalize().ToString(numberFormat);
            }
            else
            {
                if (paramType == "R" && !string.IsNullOrEmpty(function.RangeInfo.Alias))
                {
                    textValue = function.RangeInfo.Alias;
                }
                else
                {
                    textValue = (value.Value ?? 0).Normalize().ToString(numberFormat);
                }
            }

            string modifierText;
            string unitText;

            switch (paramFormat)
            {
                case "N":
                    {
                        unitText = configuration.CommandSet.UnitNames.FirstOrDefault(x => x.Value == value.Unit)?.Text;
                        modifierText = configuration.CommandSet.UnitModifiers.FirstOrDefault(x => x.Value == value.Modifier)?.Text;
                        break;
                    }
                case "U":
                    {
                        unitText = configuration.CommandSet.UnitNames.FirstOrDefault(x => x.Value == value.Unit)?.Text;
                        modifierText = string.Empty;
                        break;
                    }
                case "V":
                    {
                        unitText = string.Empty;
                        modifierText = string.Empty;
                        break;
                    }
                default:
                    {
                        throw new InvalidOperationException($"Unknown value format: \"{paramFormat}\".");
                    }
            }

            return $"{textValue}{unitSeparator}{modifierText}{unitText}";
        }

        public static bool AreValuesEqual(this IValueInfo value1, IValueInfo value2)
        {
            return GetNormal(value1) == GetNormal(value2);
        }

        public static decimal? GetNormal(this IValueInfo valueInfo)
        {
            decimal multiplier = (decimal)Math.Pow(10, (int)valueInfo.Modifier);
            return valueInfo.Value * multiplier;
        }

        public static decimal? UpdateModifier(this decimal? value, UnitModifier originalModifier, UnitModifier unitModifier)
        {
            decimal originalMultiplier = (decimal)Math.Pow(10, (int)originalModifier);
            decimal? normal = value * originalMultiplier;

            decimal multiplier = (decimal)Math.Pow(10, (int)unitModifier);
            return normal / multiplier;
        }

        public static void UpdateModifier(this IValueInfo valueInfo, UnitModifier unitModifier)
        {
            decimal? normal = valueInfo.GetNormal();
            decimal multiplier = (decimal)Math.Pow(10, (int)unitModifier);
            valueInfo.Value = normal / multiplier;
            valueInfo.Modifier = unitModifier;
        }
    }
}
