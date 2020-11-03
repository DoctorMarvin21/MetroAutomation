using MetroAutomation.Model;
using System;

namespace MetroAutomation.Automation
{
    public class DeviceProtocolClicheDisplayed : IDataObject, IDeviceProtocolClicheDisplayed
    {
        public Guid ID { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public string Grsi { get; set; }

        public string Comment { get; set; }
    }
}
