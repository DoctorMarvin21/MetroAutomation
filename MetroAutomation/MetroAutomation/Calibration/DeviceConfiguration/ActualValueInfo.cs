using LiteDB;
using System;

namespace MetroAutomation.Calibration
{
    [Serializable]
    public class ActualValueInfo
    {
        [NonSerialized]
        private string displayedValue;

        public ActualValueInfo()
        {
        }

        public ActualValueInfo(IValueInfo valueInfo)
        {
            Value = new BaseValueInfo(valueInfo);
            ActualValue = new BaseValueInfo(valueInfo);
        }

        public ActualValueInfo(IValueInfo valueInfo, string displayed)
        {
            Value = new BaseValueInfo(valueInfo);
            ActualValue = new BaseValueInfo(valueInfo);
            DisplayedValue = displayed;
        }

        [BsonIgnore]
        public string DisplayedValue
        {
            get
            {
                return displayedValue ?? Value.ToString();
            }
            set
            {
                displayedValue = value;
            }
        }

        public BaseValueInfo Value { get; set; }

        public BaseValueInfo ActualValue { get; set; }
    }
}
