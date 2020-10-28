using MetroAutomation.Calibration;
using MetroAutomation.Connection;
using MetroAutomation.ViewModel;
using System;
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

        public virtual void GetBlockHeader(DeviceProtocolItem block)
        {

        }

        public virtual DeviceProtocolItem GetProtocolRow(DeviceProtocolBlock block)
        {
            DeviceProtocolItem protolItem = new DeviceProtocolItem();

            List<BaseValueInfo> values = new List<BaseValueInfo>();

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

            values.Add(getFunction.Range);

            foreach (var standardComponent in setFunction.Components)
            {
                values.Add(standardComponent);
            }

            values.Add(setFunction.Value);
            values.Add(getFunction.Value);

            async Task ProcessFunction()
            {
                if (block.DeviceFunction.Direction == Direction.Get)
                {
                    await setFunction.Process();
                    await setFunction.Device.ChangeOutput(true, true);
                    await getFunction.Process();
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
}
