using MahApps.Metro.Controls;
using MetroAutomation.Calibration;
using System.Linq;
using System.Threading.Tasks;

namespace MetroAutomation.Automation
{
    public class ResistanceModeInfo : PairedModeInfo
    {
        protected override async Task<bool> BaseProcessFunction(MetroWindow window, DeviceProtocolBlock protocolBlock, DeviceProtocolItem protocolItem, Function baseSetFunction, Function setFunction, Function baseGetFunction, Function getFunction)
        {
            var result = await base.BaseProcessFunction(window, protocolBlock, protocolItem, baseSetFunction, setFunction, baseGetFunction, getFunction);

            if (result)
            {
                if (baseSetFunction.Components[0].Value != 0)
                {
                    var zeroRow = protocolBlock.BindableItems
                        .FirstOrDefault(x => x.Values.FirstOrDefault(y => y.Value == 0 && y is ValueInfo valueInfo && valueInfo.Function.Mode == Mode.SetRES2W && valueInfo.Type == ValueInfoType.Component) != null);
                    var zeroItem = zeroRow?.Values.FirstOrDefault(x => x is ValueInfo valueInfo && valueInfo.Type == ValueInfoType.Component && valueInfo.Function.Mode == Mode.GetRES2W);

                    if (zeroItem != null && zeroItem.Value.HasValue)
                    {
                        var temp = new BaseValueInfo(getFunction.Components[0].GetNormal() - zeroItem.GetNormal(), getFunction.Components[0].Unit, UnitModifier.None);
                        temp.UpdateModifier(getFunction.Components[0].Modifier);

                        getFunction.Components[0].FromValueInfo(temp, true);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
