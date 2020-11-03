using System;

namespace MetroAutomation.Automation
{
    public interface IDeviceProtocolClicheDisplayed
    {
        string Comment { get; set; }
        string Grsi { get; set; }
        Guid ID { get; set; }
        string Name { get; set; }
        string Type { get; set; }
    }
}