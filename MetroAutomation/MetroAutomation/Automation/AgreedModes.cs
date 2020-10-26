using MetroAutomation.Calibration;
using MetroAutomation.Connection;
using System.Collections.Generic;

namespace MetroAutomation.Automation
{
    public static class ProtocolFunctions
    {
        public static Dictionary<Mode, PairedModeInfo> PairedFunctions { get; }

        static ProtocolFunctions()
        {
            //PairedFunctions = new Dictionary<Mode, PairedInfo>
            //{
            //    {
            //        Mode.GetDCV,
            //        new PairedInfo
            //        {
            //            MeasuredMode = Mode.GetDCV,
            //            PairedMode = Mode.SetDCV
            //        }
            //    }
            //};
        }
    }

    public class PairedModeInfo
    {
        public string Name { get; set; }

        public Mode MeasureMode { get; set; }

        public Mode PairedMode { get; set; }

        public virtual void GetBlockHeader(DeviceProtocolBlock block)
        {
        }

        public virtual void GetProtocolRow(DeviceProtocolBlock block)
        {
            DeviceProtolItem protolItem = new DeviceProtolItem();

            List<BaseValueInfo> values = new List<BaseValueInfo>();

            if (!block.DeviceFunction.AutoRange)
            {
                values.Add(block.DeviceFunction.Range);
            }
            
            //if (block.Standards[0].Direction == Direction.Set)
            //{
            //    foreach (var standardComponent in block.StandardFunction.Components)
            //    {
            //        values.Add(standardComponent);
            //    }
            //}

            

            //values.Add(block.StandardFunction.Value);


        }
    }
}
