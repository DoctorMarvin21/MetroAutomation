using MetroAutomation.Calibration;
using MetroAutomation.ViewModel;
using System.Linq;

namespace MetroAutomation.FrontPanel
{
    public static class FrontPanelUtils
    {
        public static FunctionProtocol GetProtocol(Device device, Mode mode)
        {
            return new FunctionProtocol(device.Functions[mode], new BindableCollection<FunctionProtocolItem>
            {
                GetInstanceDelegate = () => CloneItem(new FunctionProtocolItem(device.Functions[mode], device.Functions[mode])),
                GetCopyDelegate = CloneItem
            });
        }

        public static FunctionProtocolItem ToItem(Function baseFunction, ValueSet valueSet)
        {
            if (valueSet.Values == null)
            {
                return new FunctionProtocolItem(baseFunction, Function.GetFunction(baseFunction.Device, baseFunction.Mode));
            }
            else
            {
                var function = Function.GetFunction(baseFunction.Device, baseFunction.Mode);

                function.ValueMultiplier = function.AvailableMultipliers?.FirstOrDefault(x => x.Multiplier == valueSet.Multiplier);

                for (int i = 0; i < valueSet.Values.Length && i < function.Components.Length; i++)
                {
                    function.Components[i].FromValueInfo(valueSet.Values[i], true);
                }

                return new FunctionProtocolItem(baseFunction, function);
            }
        }

        private static FunctionProtocolItem CloneItem(FunctionProtocolItem item)
        {
            var function = Function.GetFunction(item.BaseFunction.Device, item.BaseFunction.Mode);

            function.ValueMultiplier = item.BaseFunction.ValueMultiplier;

            for (int i = 0; i < item.Function.Components.Length; i++)
            {
                function.Components[i].FromValueInfo(item.Function.Components[i], true);
            }

            return new FunctionProtocolItem(item.BaseFunction, function);
        }
    }
}
