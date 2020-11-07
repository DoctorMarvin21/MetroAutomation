using MetroAutomation.Calibration;
using MetroAutomation.ViewModel;
using System.Linq;
using System.Threading.Tasks;

namespace MetroAutomation.FrontPanel
{
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
            BaseFunction.FromFunction(Function);

            await BaseFunction.ProcessCommand.ExecuteAsync(null);
        }

        public ValueSet ToValueSet()
        {
            return new ValueSet
            {
                ValueMultiplier = Function.ValueMultiplier?.Value,
                Values = Function.Components.Select(x => new BaseValueInfo(x)).ToArray()
            };
        }
    }
}
