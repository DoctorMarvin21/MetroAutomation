using LiteDB;
using MetroAutomation.Model;
using MetroAutomation.Editors;
using System.ComponentModel;
using System;
using MetroAutomation.Calibration;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MetroAutomation.ViewModel;

namespace MetroAutomation.FrontPanel
{
    public enum FrontPanelType
    {
        [Description("Базовая")]
        Base,
        [Description("Fluke 8508A")]
        Fluke8508,
        [Description("Fluke 5522A")]
        Fluke5520,
        [Description("Agilent 4980A")]
        Agilent4980A,
        [Description("Fluke 52120A")]
        Fluke52120A
    }

    public enum FrontPanelPosition
    {
        Left,
        Right
    }

    [Serializable]
    public class ConfigurationFrontPanel
    {
        public FrontPanelType FrontPanelType { get; set; }

        public Guid ConfigurationID { get; set; }

        public FrontPanelPosition Position { get; set; }
    }

    [Serializable]
    public class FrontPanels : IDataObject, IEditable
    {
        public ConfigurationFrontPanel[] ConfigurationFrontPanels { get; set; }

        public Guid ID { get; set; }

        public string Name { get; set; }

        [BsonIgnore]
        [field: NonSerialized]
        public bool IsEditing { get; private set; }

        [BsonIgnore]
        [field: NonSerialized]
        public NameID[] Devices { get; private set; }

        [BsonIgnore]
        [field: NonSerialized]
        public BindableCollection<ConfigurationFrontPanel> FrontPanelsLeft { get; private set; }

        [BsonIgnore]
        [field: NonSerialized]
        public ObservableCollection<ConfigurationFrontPanel> FrontPanelsRight { get; private set; }

        public void OnBeginEdit()
        {
            if (IsEditing)
            {
                return;
            }

            Devices = LiteDBAdaptor.GetNames<DeviceConfiguration>();

            FrontPanelsLeft = new BindableCollection<ConfigurationFrontPanel>(ConfigurationFrontPanels?.Where(x => x.Position == FrontPanelPosition.Left))
            {
                GetInstanceDelegate = () => new ConfigurationFrontPanel { ConfigurationID = Devices?.FirstOrDefault()?.ID ?? Guid.Empty, Position = FrontPanelPosition.Left },
                AllowDropBetweenCollections = true
            };

            FrontPanelsRight = new BindableCollection<ConfigurationFrontPanel>(ConfigurationFrontPanels?.Where(x => x.Position == FrontPanelPosition.Right))
            {
                GetInstanceDelegate = () => new ConfigurationFrontPanel { ConfigurationID = Devices?.FirstOrDefault()?.ID ?? Guid.Empty, Position = FrontPanelPosition.Right },
                AllowDropBetweenCollections = true
            };

            IsEditing = true;
        }

        public void OnEndEdit()
        {
            if (!IsEditing)
            {
                return;
            }

            List<ConfigurationFrontPanel> newPanels = new List<ConfigurationFrontPanel>();
            newPanels.AddRange(FrontPanelsLeft);
            newPanels.AddRange(FrontPanelsRight);
            ConfigurationFrontPanels = newPanels.ToArray();

            Devices = null;
            FrontPanelsLeft = null;
            FrontPanelsRight = null;

            IsEditing = false;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
