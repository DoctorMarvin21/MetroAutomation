namespace MetroAutomation.Calibration
{
    public class ComponentDescription
    {
        public string ShortName { get; set; }

        public string FullName { get; set; }

        public BaseValueInfo DefaultValue { get; set; }

        public Unit[] AllowedUnits { get; set; }
    }
}
