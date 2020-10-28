using MetroAutomation.Calibration;
using MetroAutomation.ViewModel;
using System;
using System.Threading.Tasks;

namespace MetroAutomation.Automation
{
    public class DeviceProtocolItem
    {
        public DeviceProtocolItem()
        {
            Execute = new AsyncCommandHandler(() => ProcessFunction());
        }

        public DeviceProtocolBlock Owner { get; set; }

        public BaseValueInfo[] Values { get; set; }

        public bool Accepted { get; set; }

        public IAsyncCommand Execute { get; set; }

        public Func<Task> ProcessFunction { get; set; }

        public void Update()
        {
            // TODO: implement
            if (Values?.Length == 0)
            {
                // Settings default
            }
            else
            {
                // 
            }
        }
    }
}
