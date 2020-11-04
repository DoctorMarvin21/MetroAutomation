using MetroAutomation.Model;

namespace MetroAutomation.Automation
{
    public interface IDeviceProtocolClicheDisplayed : IDataObject
    {
        string Type { get; set; }

        string Grsi { get; set; }

        string Comment { get; set; }
    }
}