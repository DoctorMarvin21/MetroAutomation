using LiteDB;
using MetroAutomation.Calibration;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace MetroAutomation.Automation
{
    [Serializable]
    public class DeviceProtocolItem : INotifyPropertyChanged
    {
        [NonSerialized]
        private bool isSelected = true;

        [BsonIgnore]
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        [BsonIgnore]
        [field: NonSerialized]
        public BaseValueInfo[] Values { get; set; }

        public BaseValueInfo[] StoredValues { get; set; }

        [BsonIgnore]
        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }

        [BsonIgnore]
        [field: NonSerialized]
        public Func<Task> ProcessFunction { get; set; }
    }
}
