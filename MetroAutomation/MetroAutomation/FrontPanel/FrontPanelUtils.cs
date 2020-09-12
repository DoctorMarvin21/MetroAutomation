using MetroAutomation.Calibration;
using MetroAutomation.ViewModel;

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

        private static FunctionProtocolItem CloneItem(FunctionProtocolItem item)
        {
            var function = Function.GetFunction(item.BaseFunction.Device, item.BaseFunction.Mode);

            for (int i = 0; i < item.Function.Components.Length; i++)
            {
                function.Components[i].FromValueInfo(item.Function.Components[i], true);
            }

            return new FunctionProtocolItem(item.BaseFunction, function);
        }
    }
}
