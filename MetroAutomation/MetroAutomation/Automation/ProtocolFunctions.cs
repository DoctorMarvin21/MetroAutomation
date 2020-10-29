using MetroAutomation.Calibration;
using MetroAutomation.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MetroAutomation.Automation
{
    public enum AutomationMode
    {
        GetDCV,
        SetDCV,
        GetACV,
        SetACV,
        GetDCI,
        SetDCI,
        GetACI,
        SetACI,
        GetRES2W,
        SetRES2W,
        GetRES4W,
        SetRES4W,
        GetCAP2W,
        SetCAP2W,
        GetCAP4W,
        SetCAP4W,
        GetIND2W,
        SetIND2W,
        GetIND4W,
        SetIND4W,
        GetADM4W,
        SetADM4W,
        GetDCP,
        SetDCP,
        GetACP,
        SetACP,
        SetDCV_DCV,
        SetACV_ACV,
        GetTEMP,
        SetTEMP
    }

    public static class ProtocolFunctions
    {
        public static Dictionary<AutomationMode, PairedModeInfo> PairedFunctions { get; }

        static ProtocolFunctions()
        {
            PairedFunctions = new Dictionary<AutomationMode, PairedModeInfo>
            {
                {
                    AutomationMode.GetDCV,
                    new PairedModeInfo
                    {
                        AutomationMode = AutomationMode.GetDCV,
                        SourceMode = Mode.GetDCV,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.GetDCV, DescriptionType.Full),
                        Standards = new[] { new StandardInfo("Калибратор напряжения", Mode.SetDCV) }
                    }
                },
                {
                    AutomationMode.GetACV,
                    new PairedModeInfo
                    {
                        AutomationMode = AutomationMode.GetACV,
                        SourceMode = Mode.GetACV,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.GetACV, DescriptionType.Full),
                        Standards = new[] { new StandardInfo("Калибратор напряжения", Mode.SetACV) }
                    }
                }
            };
        }

        public static PairedModeInfo GetPairedModeInfo(DeviceProtocolBlock deviceProtocolBlock)
        {
            if (PairedFunctions.TryGetValue(deviceProtocolBlock.AutomationMode, out var mode))
            {
                return mode;
            }
            else
            {
                // Not good a good practice
                return new PairedModeInfo();
            }
        }

        public static StandardInfo GetStandardInfo(DeviceProtocolBlock deviceProtocolBlock, int index)
        {
            var modeInfo = GetPairedModeInfo(deviceProtocolBlock);

            if (modeInfo.Standards?.Length > index)
            {
                return modeInfo.Standards[index];
            }
            else
            {
                return null;
            }
        }

        public static PairedModeInfo[] GetModeInfo(Device device)
        {
            return PairedFunctions.Values.Where(x => device.Functions.Values.Any(y => y.Mode == x.SourceMode)).ToArray();
        }
    }

    public class PairedModeInfo
    {
        public string Name { get; set; }

        public AutomationMode AutomationMode { get; set; }

        public Mode SourceMode { get; set; }

        public StandardInfo[] Standards { get; set; }

        public virtual DeviceColumnHeader[] GetBlockHeaders(DeviceProtocolBlock block)
        {
            Function setFunction;
            Function getFunction;

            if (block.DeviceFunction.Direction == Direction.Get)
            {
                getFunction = block.DeviceFunction;
                setFunction = block.Standards[0].Function;
            }
            else
            {
                getFunction = block.Standards[0].Function;
                setFunction = block.DeviceFunction;
            }

            List<DeviceColumnHeader> result = new List<DeviceColumnHeader>();

            if (!getFunction.AutoRange)
            {
                result.Add(new DeviceColumnHeader(0, "Диапазон (изм.)"));
            }

            int setComponentsIndex = getFunction.Components.Length + 5;
            foreach (var setComponent in setFunction.Components)
            {
                result.Add(new DeviceColumnHeader(setComponentsIndex, $"{FunctionDescription.GetDescription(setComponent).FullName} (уст.)"));
                setComponentsIndex++;
            }

            if (setFunction.AvailableMultipliers != null)
            {
                result.Add(new DeviceColumnHeader(getFunction.Components.Length + setFunction.Components.Length + 6, "Множитель (уст.)"));
            }

            if (setFunction.AvailableMultipliers != null || !FunctionDescription.IsSingleComponent(setFunction))
            {
                result.Add(new DeviceColumnHeader(getFunction.Components.Length + setFunction.Components.Length + 7, $"{FunctionDescription.GetDescription(setFunction.Value).FullName} (уст.)"));
            }

            int getComponentsIndex = 1;

            foreach (var getComponent in getFunction.Components)
            {
                result.Add(new DeviceColumnHeader(getComponentsIndex, $"{FunctionDescription.GetDescription(getComponent).FullName} (изм.)"));
                getComponentsIndex++;
            }

            if (getFunction.AvailableMultipliers != null)
            {
                result.Add(new DeviceColumnHeader(getFunction.Components.Length + 2, "Множитель (изм.)"));
            }

            if (getFunction.AvailableMultipliers != null || !FunctionDescription.IsSingleComponent(getFunction))
            {
                result.Add(new DeviceColumnHeader(getFunction.Components.Length + 3, $"{FunctionDescription.GetDescription(getFunction.Value).FullName} (изм.)"));
            }

            return result.ToArray();
        }

        public virtual DeviceProtocolItem GetProtocolRow(DeviceProtocolBlock block)
        {
            DeviceProtocolItem protolItem = new DeviceProtocolItem();

            List<BaseValueInfo> values = new List<BaseValueInfo>();

            Function baseSetFunction;
            Function baseGetFunction;

            if (block.DeviceFunction.Direction == Direction.Get)
            {
                baseGetFunction = block.DeviceFunction;
                baseSetFunction = block.Standards[0].Function;
            }
            else
            {
                baseGetFunction = block.Standards[0].Function;
                baseSetFunction = block.DeviceFunction;
            }

            Function setFunction = Function.GetFunction(baseSetFunction.Device, baseSetFunction.Mode);
            Function getFunction = Function.GetFunction(baseGetFunction.Device, baseGetFunction.Mode);

            FillBlock(getFunction, values);
            FillBlock(setFunction, values);

            async Task ProcessFunction()
            {
                baseSetFunction.FromFunction(setFunction);

                await baseSetFunction.Process();

                if (!baseSetFunction.Device.IsOutputOn)
                {
                    await baseSetFunction.Device.ChangeOutput(true, true);
                }

                baseGetFunction.FromFunction(getFunction);
                await baseGetFunction.Process();

                getFunction.FromFunction(baseGetFunction);
            }

            return new DeviceProtocolItem
            {
                ProcessFunction = ProcessFunction,
                Values = values.ToArray()
            };
        }

        private void FillBlock(Function function, List<BaseValueInfo> infos)
        {
            infos.Add(function.Range);

            foreach (ValueInfo component in function.Components)
            {
                infos.Add(component);
            }

            // Will be always invisible, but stored
            infos.Add(function.Value);

            infos.Add(new MultiplierValueInfo(function));

            // Just to display
            infos.Add(function.MultipliedValue);
        }

        public virtual DeviceProtocolItem GetProtocolRowCopy(DeviceProtocolBlock block, DeviceProtocolItem source)
        {
            var newItem = GetProtocolRow(block);

            for (int i = 0; i < newItem.Values.Length; i++)
            {
                if (newItem.Values[i] is IReadOnlyValueInfo valueInfo && !valueInfo.IsReadOnly)
                {
                    valueInfo.FromValueInfo(source.Values[i], true);
                }
                else
                {
                    newItem.Values[i].FromValueInfo(source.Values[i], true);
                }
            }

            return newItem;
        }
    }

    public class StandardInfo
    {
        public StandardInfo(string description, Mode mode)
        {
            Description = description;
            Mode = mode;
        }

        public string Description { get; set; }

        public Mode Mode { get; set; }
    }

    public class DeviceColumnHeader
    {
        public DeviceColumnHeader(int index, string name)
        {
            Index = index;
            Name = name;
        }

        public int Index { get; }

        public string Name { get; }
    }
}
