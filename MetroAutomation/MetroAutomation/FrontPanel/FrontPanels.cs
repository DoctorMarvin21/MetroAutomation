using LiteDB;
using MetroAutomation.Model;
using MetroAutomation.Editors;
using System.ComponentModel;
using System;
using MetroAutomation.Calibration;

namespace MetroAutomation.FrontPanel
{
    public enum FrontPanelType
    {
        [Description("Без панели")]
        None,
        [Description("Базовая")]
        Base,
        [Description("Fluke 8508A")]
        Fluke8508,
        [Description("Fluke 5522A")]
        Fluke5520,
        [Description("Agilent 4980A")]
        Agilent4980A
    }

    [Serializable]
    public class ConfigurationFrontPanel
    {
        public FrontPanelType FrontPanelType { get; set; }

        public int ConfigurationID { get; set; }
    }

    [Serializable]
    public class FrontPanels : IDataObject, IEditable
    {
        public ConfigurationFrontPanel[] ConfigurationFrontPanels { get; set; }

        public int ID { get; set; }

        public string Name { get; set; }

        [BsonIgnore]
        [field: NonSerialized]
        public NameID[] Devices { get; private set; }

        public void OnBeginEdit()
        {
            Devices = LiteDBAdaptor.GetNames<DeviceConfiguration>();

            if (ConfigurationFrontPanels == null)
            {
                ConfigurationFrontPanels = new ConfigurationFrontPanel[4];

                for (int i = 0; i < ConfigurationFrontPanels.Length; i++)
                {
                    ConfigurationFrontPanels[i] = new ConfigurationFrontPanel();
                }
            }
        }

        public void OnEndEdit()
        {
            Devices = null;
        }
    }
}
