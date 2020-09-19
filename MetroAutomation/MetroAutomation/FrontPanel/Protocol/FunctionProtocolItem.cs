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
            for (int i = 0; i < BaseFunction.Components.Length; i++)
            {
                ValueInfo component = BaseFunction.Components[i];
                component.FromValueInfo(Function.Components[i], true);
            }

            BaseFunction.ValueMultiplier = Function.ValueMultiplier;

            await BaseFunction.ProcessCommand.ExecuteAsync(null);
        }

        public ValueSet ToValueSet()
        {
            return new ValueSet
            {
                Multiplier = Function.ValueMultiplier?.Multiplier,
                Values = Function.Components.Select(x => new BaseValueInfo(x)).ToArray()
            };
        }
    }
}
