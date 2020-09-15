using LiteDB;
using MetroAutomation.ViewModel;
using System;

namespace MetroAutomation.Calibration
{
    [Serializable]
    public class ValueRange
    {
        public ValueRange()
        {
        }

        public ValueRange(BaseValueInfo min, BaseValueInfo max)
        {
            Min = min;
            Max = max;
        }

        [BsonIgnore]
        public string Description => Min?.Unit.GetDescription(DescriptionType.Full) ?? Max?.Unit.GetDescription(DescriptionType.Full);

        public BaseValueInfo Min { get; set; }

        public BaseValueInfo Max { get; set; }

        public bool FitsRange(IValueInfo valueInfo)
        {
            var normal = valueInfo.GetNormal();
            var convertedMin = FunctionDescription.UnitConverter(normal, valueInfo.Unit, Min.Unit);
            var convertedMax = FunctionDescription.UnitConverter(normal, valueInfo.Unit, Max.Unit);

            return convertedMin >= Min.GetNormal()
                && convertedMax <= Max.GetNormal();
        }
    }
}
