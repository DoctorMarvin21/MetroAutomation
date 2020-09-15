using LiteDB;
using MetroAutomation.ViewModel;
using System;

namespace MetroAutomation.Calibration
{
    [Serializable]
    public class ModeInfo
    {
        public ModeInfo()
        {
        }

        public ModeInfo(Mode mode)
        {
            Mode = mode;
        }

        public Mode Mode { get; set; }

        public bool IsAvailable { get; set; }

        public RangeInfo[] Ranges { get; set; }

        public ActualValueInfo[] ActualValues { get; set; }

        public ValueMultiplier[] Multipliers { get; set; }

        [BsonIgnore]
        [field: NonSerialized]
        public BindableCollection<RangeInfo> BindableRanges { get; set; }

        [BsonIgnore]
        [field: NonSerialized]
        public BindableCollection<ActualValueInfo> BindableActualValues { get; set; }

        [BsonIgnore]
        [field: NonSerialized]
        public BindableCollection<ValueMultiplier> BindableMultipliers { get; set; }
    }
}
