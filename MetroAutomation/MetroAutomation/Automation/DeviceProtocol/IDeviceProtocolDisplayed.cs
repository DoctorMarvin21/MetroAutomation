using System;

namespace MetroAutomation.Automation
{
    public interface IDeviceProtocolDisplayed
    {
        DateTime CalibrationDate { get; set; }
        string DeviceOwner { get; set; }
        string Grsi { get; set; }
        int ID { get; set; }
        string Name { get; set; }
        string ProtocolNumber { get; set; }
        string SerialNumber { get; set; }
        string Type { get; set; }
        WorkStatus WorkStatus { get; set; }
    }
}