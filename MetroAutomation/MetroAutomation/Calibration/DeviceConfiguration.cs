using LiteDB;
using MetroAutomation.Model;
using MetroAutomation.Editors;
using MetroAutomation.ViewModel;
using System;
using System.Linq;

namespace MetroAutomation.Calibration
{
    [Serializable]
    public class DeviceConfiguration : IDataObject, IEditable
    {
        [NonSerialized]
        private CommandSet commandSet;

        public int ID { get; set; }

        public string Name { get; set; }

        public bool IsStandard { get; set; }

        public ConnectionSettings DefaultConnectionSettings { get; set; }
            = new ConnectionSettings();

        public ModeInfo[] ModeInfo { get; set; }

        public int CommandSetID { get; set; }

        [BsonIgnore]
        public CommandSet CommandSet
        {
            get
            {
                return commandSet;
            }
            set
            {
                commandSet = value;

                if (CommandSet != null)
                {
                    CommandSetID = CommandSet.ID;
                }
            }
        }

        [BsonIgnore]
        [field: NonSerialized]
        public NameID[] AvailableCommandSets { get; private set; }

        public bool TryGetRanges(Mode mode, out RangeInfo[] ranges)
        {
            ranges = ModeInfo.FirstOrDefault(x => x.Mode == mode)?.Ranges;
            return ranges != null;
        }

        public void OnBeginEdit()
        {
            AvailableCommandSets = LiteDBAdaptor.GetNames<CommandSet>();

            ModeInfo = EnumExtensions.GetValues<Mode>()
                .Where(x => ModeInfo?.FirstOrDefault(y => y.Mode == x) == null)
                .Select(x => new ModeInfo(x))
                .Union(ModeInfo ?? new ModeInfo[0])
                .OrderBy(x => x.Mode)
                .ToArray();

            foreach (ModeInfo info in ModeInfo)
            {
                info.BindableRanges = new BindableCollection<RangeInfo>(info.Ranges ?? new RangeInfo[0])
                {
                    GetInstanceDelegate = () => FunctionDescription.GetDefaultRangeInfo(info.Mode)
                };

                info.BindableActualValues = new BindableCollection<ActualValueInfo>(info.ActualValues ?? new ActualValueInfo[0])
                {
                    GetInstanceDelegate = () => FunctionDescription.GetDefaultActualValue(info.Mode)
                };

                info.BindableMultipliers = new BindableCollection<ValueMultiplier>(info.Multipliers ?? new ValueMultiplier[0])
                {
                    GetInstanceDelegate = () => new ValueMultiplier("Множитель", 1)
                };
            }
        }

        public void OnEndEdit()
        {
            AvailableCommandSets = null;

            if (ModeInfo != null)
            {
                foreach (var info in ModeInfo)
                {
                    info.Ranges = info.BindableRanges.ToArray();
                    info.ActualValues = info.BindableActualValues.ToArray();
                    info.Multipliers = info.BindableMultipliers.ToArray();

                    if (info.Ranges.Length == 0)
                    {
                        info.Ranges = null;
                    }

                    if (info.ActualValues.Length == 0)
                    {
                        info.ActualValues = null;
                    }

                    if (info.Multipliers.Length == 0)
                    {
                        info.Multipliers = null;
                    }

                    info.BindableRanges = null;
                    info.BindableActualValues = null;
                    info.BindableMultipliers = null;
                }

                ModeInfo = ModeInfo
                    .Where(x => x.Ranges?.Length > 0 || x.ActualValues?.Length > 0 || x.Multipliers?.Length > 0)
                    .ToArray();
            }
        }
    }

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

    [Serializable]
    public class ValueMultiplier
    {
        public ValueMultiplier()
        {
        }

        public ValueMultiplier(string name, decimal multiplier)
        {
            Name = name;
            Multiplier = multiplier;
        }

        public string Name { get; set; }

        public decimal Multiplier { get; set; }
    }

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

    [Serializable]
    public class ActualValueInfo
    {
        public ActualValueInfo()
        {
        }

        public ActualValueInfo(IValueInfo valueInfo)
        {
            Value = new BaseValueInfo(valueInfo);
            ActualValue = new BaseValueInfo(valueInfo);
        }

        public BaseValueInfo Value { get; set; }

        public BaseValueInfo ActualValue { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is ActualValueInfo valueInfo)
            {
                if (Value == null && valueInfo.Value == null)
                {
                    return true;
                }
                else
                {
                    return valueInfo.Value?.Equals(this?.Value) ?? false;
                }
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }
    }

    [Serializable]
    public class RangeInfo
    {
        public string Output { get; set; }

        public string Alias { get; set; }

        public BaseValueInfo Range { get; set; }

        public ValueRange[] ComponentsRanges { get; set; }
    }
}
