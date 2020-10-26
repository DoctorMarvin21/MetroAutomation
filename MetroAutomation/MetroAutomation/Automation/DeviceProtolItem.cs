using MetroAutomation.Calibration;
using MetroAutomation.ViewModel;
using System;
using System.Threading.Tasks;

namespace MetroAutomation.Automation
{
    public class DeviceProtolItem
    {
        private DeviceProtocolBlock owner;

        public DeviceProtolItem()
        {
            Execute = new AsyncCommandHandler(() => ProcessFunction());
        }

        public DeviceProtocolBlock Owner
        {
            get
            {
                return owner;
            }
            set
            {
                owner = value;
                Update();
            }
        }

        public BaseValueInfo[] Values { get; set; }

        public bool Accepted { get; set; }

        public IAsyncCommand Execute { get; set; }

        public Func<Task> ProcessFunction { get; set; }

        public void Update()
        {
        }
    }
}
