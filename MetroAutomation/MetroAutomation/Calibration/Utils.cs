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
                    RangeInfo range = ranges.FirstOrDefault(x => ValueInfoUtils.AreValuesEqual(x.Range, function.Range));

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
            int paramIndex;

            if (argData.Length > 1 && int.TryParse(argData[1], out int parsedIndex))
            {
                paramIndex = parsedIndex - 1;
            }
            else
            {
                paramIndex = 0;
            }

            string paramFormat;

            if (argData.Length > 2)
            {
                paramFormat = argData[2];
            }
            else
            {
                paramFormat = "N";
            }

            Unit? desiredUnitType;

            if (argData.Length > 3 && Enum.TryParse(argData[3], true, out Unit parsedUnit))
            {
                desiredUnitType = parsedUnit;
            }
            else
            {
                desiredUnitType = null;
            }

            string unitSeparator;

            if (argData.Length > 4)
            {
                unitSeparator = argData[4];
            }
            else
            {
                unitSeparator = " ";
            }

            UnitModifier? desiredUnitModifier;

            if (argData.Length > 5 && Enum.TryParse(argData[5], true, out UnitModifier parsedModifier))
            {
                desiredUnitModifier = parsedModifier;
            }
            else
            {
                desiredUnitModifier = null;
            }

            string decimalSeparator;

            if (argData.Length > 6)
            {
                decimalSeparator = argData[6];
            }
            else
            {
                decimalSeparator = ".";
            }

            IValueInfo value;

            switch (paramType)
            {
                case "V":
                default:
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
            }

            if (!desiredUnitType.HasValue && paramFormat.StartsWith("R"))
            {
                desiredUnitType = function.RangeInfo.Range.Unit;
            }

            if (desiredUnitType.HasValue)
            {
                var newValue = FunctionDescription.UnitConverter(value.GetNormal(), value.Unit, desiredUnitType.Value);
                value = new BaseValueInfo(newValue, desiredUnitType.Value, UnitModifier.None);
            }

            if (desiredUnitModifier.HasValue)
            {
                var modified = ValueInfoUtils.UpdateModifier(value.Value, value.Modifier, desiredUnitModifier.Value);
                value = new BaseValueInfo(modified, value.Unit, desiredUnitModifier.Value);
            }

            if (paramFormat.StartsWith("R"))
            {
                var rangeModified = ValueInfoUtils.UpdateModifier(value.Value, value.Modifier, function.RangeInfo.Range.Modifier);
                value = new BaseValueInfo(rangeModified, value.Unit, function.RangeInfo.Range.Modifier);

                paramFormat = paramFormat.Replace("R", string.Empty);
            }

            NumberFormatInfo numberFormat = new NumberFormatInfo
            {
                NumberDecimalSeparator = decimalSeparator
            };

            string textValue;

            if (paramFormat != "N")
            {
                if (paramType == "R" && !string.IsNullOrEmpty(function.RangeInfo.Alias))
                {
                    textValue = function.RangeInfo.Alias;
                }
                else
                {
                    textValue = (value.GetNormal() ?? 0).Normalize().ToString(numberFormat);
                }
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
                default:
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
            }

            if (string.IsNullOrEmpty(unitText) && string.IsNullOrEmpty(modifierText))
            {
                unitSeparator = string.Empty;
            }

            return $"{textValue}{unitSeparator}{modifierText}{unitText}";
        }
    }
}
