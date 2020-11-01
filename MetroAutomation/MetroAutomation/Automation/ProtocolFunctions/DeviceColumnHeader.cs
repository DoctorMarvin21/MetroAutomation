namespace MetroAutomation.Automation
{
    public class DeviceColumnHeader
    {
        public DeviceColumnHeader(int index, string name)
        {
            Index = index;
            Name = name;
        }

        public int Index { get; }

        public string Name { get; }
    }
}
