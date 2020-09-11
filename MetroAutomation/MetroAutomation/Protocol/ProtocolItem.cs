using MetroAutomation.Calibration;
using MetroAutomation.ViewModel;
using System;
using System.Threading.Tasks;

namespace MetroAutomation.Protocol
{
    public class ProtocolItem
    {
        public ProtocolItem(Func<Task> executeDelegate, ValueInfo[] values)
        {
            ExecuteDelegate = executeDelegate;
            ExecuteCommand = new AsyncCommandHandler(ExecuteDelegate);
            Values = values;
        }

        public IAsyncCommand ExecuteCommand { get; }

        public Func<Task> ExecuteDelegate { get;}

        public ValueInfo[] Values { get; }
    }
}
