using LiteDB;
using MetroAutomation.Model;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MetroAutomation.Automation
{
    [Serializable]
    public class DeviceProtocolCliche : IDataObject, INotifyPropertyChanged
    {
        private int configurationID;
        private string name;
        private string type;
        private string grsi;

        [BsonIgnore]
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public int ID { get; set; }

        public int ConfigurationID
        {
            get
            {
                return configurationID;
            }
            set
            {
                configurationID = value;
                OnPropertyChanged();
                OnConfigurationIDChanged();
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        public string Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
                OnPropertyChanged();
            }
        }

        public string Grsi
        {
            get
            {
                return grsi;
            }
            set
            {
                grsi = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnConfigurationIDChanged()
        {
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
