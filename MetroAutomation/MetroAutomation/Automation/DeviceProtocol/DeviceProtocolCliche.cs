using LiteDB;
using MetroAutomation.Model;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MetroAutomation.Automation
{
    [Serializable]
    public class DeviceProtocolCliche: IDataObject, INotifyPropertyChanged
    {
        private int configurationID;

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

        public string Name { get; set; }

        public string Type { get; set; }

        protected virtual void OnConfigurationIDChanged()
        {
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
