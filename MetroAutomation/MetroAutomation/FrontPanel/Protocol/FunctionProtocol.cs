using MetroAutomation.Calibration;
using MetroAutomation.ViewModel;
using System.Linq;

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

        public void FromValueSet(FunctionValueSet valueSet)
        {
            if (valueSet.Values != null)
            {
                foreach (var set in valueSet.Values)
                {
                    Items.Add(FrontPanelUtils.ToItem(OriginalFuntion, set));
                }
            }
        }

        public FunctionValueSet ToValueSet()
        {
            if (Items.Count == 0)
            {
                return null;
            }
            else
            {
                return new FunctionValueSet
                {
                    Mode = OriginalFuntion.Mode,
                    Values = Items.Select(x => x.ToValueSet()).ToArray()
                };
            }
        }
    }
}
