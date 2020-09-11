using MetroAutomation.Calibration;
using MetroAutomation.ViewModel;

namespace MetroAutomation.FrontPanel
{
    public static class FrontPanelUtils
    {
        public static FunctionProtocol GetProtocol(Device device, Mode mode)
        {
            return new FunctionProtocol(device.Functions[mode], new BindableCollection<Function>
            {
                GetInstanceDelegate = () => CloneFunction(device.Functions[mode]),
                GetCopyDelegate = CloneFunction
            });
        }

        private static Function CloneFunction(Function originalFunction)
        {
            var function = Function.GetFunction(originalFunction.Device, originalFunction.Mode);

            for (int i = 0; i < originalFunction.Components.Length; i++)
            {
                function.Components[i].FromValueInfo(originalFunction.Components[i], true);
            }

            return function;
        }
    }
}
