using LiteDB;
using MetroAutomation.Calibration;
using MetroAutomation.ViewModel;
using System;
using System.Threading.Tasks;

namespace MetroAutomation.Automation
{
    [Serializable]
    public class DeviceProtocolItem
    {
        public DeviceProtocolItem()
        {
            Execute = new AsyncCommandHandler(() => ProcessFunction());
        }

        [BsonIgnore]
        [field: NonSerialized]
        public BaseValueInfo[] Values { get; set; }

        public BaseValueInfo[] StoredValues { get; set; }

        [BsonIgnore]
        [field: NonSerialized]
        public IAsyncCommand Execute { get; set; }

        [BsonIgnore]
        [field: NonSerialized]
        public Func<Task> ProcessFunction { get; set; }
    }
}
