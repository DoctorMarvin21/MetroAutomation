using MetroAutomation.Calibration;
using MetroAutomation.ViewModel;

namespace MetroAutomation.FrontPanel
{
    public class FunctionProtocol
    {
        public FunctionProtocol(Function originalFuntion, BindableCollection<Function> items)
        {
            OriginalFuntion = originalFuntion;
            Items = items;
        }

        public Function OriginalFuntion { get; }

        public BindableCollection<Function> Items { get; }
    }
}
