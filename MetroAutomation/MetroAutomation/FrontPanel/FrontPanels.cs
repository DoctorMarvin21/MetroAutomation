using LiteDB;
using MetroAutomation.Model;
using MetroAutomation.Editors;
using System.ComponentModel;

namespace MetroAutomation.FrontPanel
{
    public enum FrontPanelType
    {
        [Description("Калибратор")]
        Calibrator,
        [Description("Fluke 8508A")]
        Fluke8508
    }

    public class ConfigurationFrontPanel
    {
        public FrontPanelType FrontPanelType { get; set; }

        public int ConfigurationID { get; set; }
    }

    public class FrontPanels : IDataObject, IEditable
    {
        public ConfigurationFrontPanel[] ConfigurationFrontPanels { get; set; }

        public int ID { get; set; }

        public string Name { get; set; }

        [BsonIgnore]
        public NameID[] Standards { get; private set; }

        public void OnBeginEdit()
        {
            Standards = LiteDBAdaptor.GetStandardNames();

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
            Standards = null;
        }

        public void Save()
        {
            LiteDBAdaptor.SaveData(this);
        }

        public static FrontPanels Load()
        {
            return LiteDBAdaptor.LoadData<FrontPanels>(1) ?? new FrontPanels();
        }
    }
}
