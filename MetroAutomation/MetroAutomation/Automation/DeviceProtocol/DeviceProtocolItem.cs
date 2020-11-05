using LiteDB;
using MetroAutomation.Calibration;
using MetroAutomation.Controls;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MetroAutomation.Automation
{
    [Serializable]
    public class DeviceProtocolItem : INotifyPropertyChanged
    {
        [NonSerialized]
        private bool isSelected = true;

        [NonSerialized]
        private bool hasErrors;

        [NonSerialized]
        private LedState status;

        [NonSerialized]
        private BaseValueInfo[] values;

        [NonSerialized]
        private string statusText;

        [BsonIgnore]
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        [BsonIgnore]
        public BaseValueInfo[] Values
        {
            get
            {
                return values;
            }
            set
            {
                values = value;

                if (values != null)
                {
                    foreach (BaseValueInfo valueInfo in Values)
                    {
                        valueInfo.PropertyChanged += ValuePropertyChanged;
                    }
                }

                OnPropertyChanged();
                UpdateStatus();
            }
        }

        [BsonIgnore]
        public bool HasErrors
        {
            get
            {
                return hasErrors;
            }
            private set
            {
                hasErrors = value;
                OnPropertyChanged();
            }
        }

        public BaseValueInfo[] StoredValues { get; set; }

        [BsonIgnore]
        public LedState Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
                OnPropertyChanged();
            }
        }

        [BsonIgnore]
        public string StatusText
        {
            get
            {
                return statusText;
            }
            set
            {
                statusText = value;
                OnPropertyChanged();
            }
        }

        private void ValuePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            if (values != null)
            {
                HasErrors = values.OfType<ValueInfo>().Any(x => !x.IsReadOnly && x.HasErrors);

                if (HasErrors)
                {
                    Status = LedState.Fail;
                    StatusText = "Одно или несколько значений выходят за границы диапазонов";
                }
                else
                {
                    var result = values.OfType<ResultValueInfo>().FirstOrDefault();

                    if (result != null)
                    {
                        Status = result.Status;
                        StatusText = result.TextValue;
                    }
                }
            }
        }

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
                OnPropertyChanged();
            }
        }

        [BsonIgnore]
        [field: NonSerialized]
        public Func<Task<bool>> ProcessFunction { get; set; }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
