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

        [BsonIgnore]
        [field: NonSerialized]
        public bool IsEditing { get; private set; }

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
            ranges = ModeInfo?.FirstOrDefault(x => x.Mode == mode)?.Ranges;
            return ranges != null;
        }

        public void OnBeginEdit()
        {
            if (IsEditing)
            {
                return;
            }

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

            IsEditing = true;
        }

        public void OnEndEdit()
        {
            if (!IsEditing)
            {
                return;
            }

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
                    .Where(x => x.IsAvailable || x.Ranges?.Length > 0 || x.ActualValues?.Length > 0 || x.Multipliers?.Length > 0)
                    .ToArray();
            }

            IsEditing = false;
        }
    }
}
