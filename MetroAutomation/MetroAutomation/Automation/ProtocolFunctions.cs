using MetroAutomation.Calibration;
using MetroAutomation.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MetroAutomation.Automation
{
    public static class ProtocolFunctions
    {
        public static Dictionary<Mode, PairedModeInfo> PairedFunctions { get; }

        static ProtocolFunctions()
        {
            PairedFunctions = new Dictionary<Mode, PairedModeInfo>
            {
                {
                    Mode.GetDCV,
                    new PairedModeInfo
                    {
                        MeasureMode = Mode.GetDCV,
                        PairedMode = Mode.SetDCV,
                        Name = ExtendedDescriptionAttribute.GetDescription(Mode.GetDCV, DescriptionType.Full)
                    }
                }
            };
        }
    }

    public class PairedModeInfo
    {
        public string Name { get; set; }

        public Mode MeasureMode { get; set; }

        public Mode PairedMode { get; set; }

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
            result.Add(new DeviceColumnHeader { Name = "Диапазон", IsVisible = true });


            foreach (var standardComponent in setFunction.Components)
            {
                result.Add(new DeviceColumnHeader { Name = FunctionDescription.GetDescription(standardComponent).FullName, IsVisible = true });
            }

            result.Add(new DeviceColumnHeader { Name = FunctionDescription.GetDescription(setFunction.Value).FullName, IsVisible = true });
            result.Add(new DeviceColumnHeader { Name = FunctionDescription.GetDescription(getFunction.Value).FullName, IsVisible = true });

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

            values.Add(getFunction.Range);

            foreach (var standardComponent in setFunction.Components)
            {
                values.Add(standardComponent);
            }

            //values.Add(new BaseValueInfo { Value = values. });

            values.Add(setFunction.Value);
            values.Add(getFunction.Value);

            async Task ProcessFunction()
            {
                if (block.DeviceFunction.Direction == Direction.Get)
                {
                    baseSetFunction.FromFunction(setFunction);

                    await baseSetFunction.Process();
                    await baseSetFunction.Device.ChangeOutput(true, true);

                    baseGetFunction.FromFunction(getFunction);
                    await baseGetFunction.Process();

                    getFunction.FromFunction(baseGetFunction);
                }
            }

            return new DeviceProtocolItem
            {
                Owner = block,
                ProcessFunction = ProcessFunction,
                Values = values.ToArray()
            };
        }
    }

    public class DeviceColumnHeader
    {
        public string Name { get; set; }

        public bool IsVisible { get; set; }
    }
}
