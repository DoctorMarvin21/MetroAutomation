using MetroAutomation.Calibration;
using MetroAutomation.ViewModel;
using System.Threading.Tasks;

namespace MetroAutomation.FrontPanel
{
    public class FunctionProtocol
    {
        public FunctionProtocol(Function originalFuntion, BindableCollection<FunctionProtocolItem> items)
        {
            OriginalFuntion = originalFuntion;
            AvailableMultipliers = originalFuntion.AvailableMultipliers;
            Items = items;
        }

        public ValueMultiplier[] AvailableMultipliers { get; }

        public Function OriginalFuntion { get; }

        public BindableCollection<FunctionProtocolItem> Items { get; }
    }

    public class FunctionProtocolItem
    {
        public FunctionProtocolItem(Function baseFunction, Function function)
        {
            BaseFunction = baseFunction;
            Function = function;
            Command = new AsyncCommandHandler(CommandHandler);
        }

        public Function BaseFunction { get; }

        public Function Function { get; }

        public IAsyncCommand Command { get; set; }

        private async Task CommandHandler()
        {
            for (int i = 0; i < BaseFunction.Components.Length; i++)
            {
                ValueInfo component = BaseFunction.Components[i];
                component.FromValueInfo(Function.Components[i], true);
            }

            BaseFunction.CurrentMultiplier = Function.CurrentMultiplier;

            await BaseFunction.ProcessCommand.ExecuteAsync(null);
        }
    }
}
