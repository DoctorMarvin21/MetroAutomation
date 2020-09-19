using LiteDB;
using MetroAutomation.Editors;
using MetroAutomation.Model;
using System;
using System.Linq;

namespace MetroAutomation.Calibration
{
    public enum FunctionCommandType
    {
        Function,
        Range,
        Value
    }

    [Serializable]
    public class CommandSet : IDataObject, IEditable
    {
        public int ID { get; set; }

        public string Name { get; set; }

        [BsonIgnore]
        [field: NonSerialized]
        public bool IsEditing { get; private set; }

        public ValueText<UnitModifier>[] UnitModifiers { get; set; }

        public ValueText<Unit>[] UnitNames { get; set; }

        public FunctionCommandSet[] FunctionCommands { get; set; }

        public string ConnectCommand { get; set; }

        public string DisconnectCommand { get; set; }

        public string OutputOnCommand { get; set; }

        public string OutputOffCommand { get; set; }

        public bool WaitForActionResponse { get; set; }

        public string ActionSuccess { get; set; }

        public string ActionFail { get; set; }

        public bool TryGetCommand(Mode mode, FunctionCommandType commandType, out string command)
        {
            var set = FunctionCommands?.FirstOrDefault(x => x.Mode == mode);

            if (set != null)
            {
                switch (commandType)
                {
                    case FunctionCommandType.Function:
                        {
                            command = set.FunctionCommand;
                            break;
                        }
                    case FunctionCommandType.Range:
                        {
                            command = set.RangeCommand;
                            break;
                        }
                    case FunctionCommandType.Value:
                        {
                            command = set.ValueCommand;
                            break;
                        }
                    default:
                        {
                            command = null;
                            break;
                        }
                }

                return !string.IsNullOrEmpty(command);
            }
            else
            {
                command = null;
                return false;
            }
        }

        public bool CheckResponse(string response)
        {
            if (!WaitForActionResponse)
            {
                return true;
            }
            if (string.IsNullOrWhiteSpace(ActionSuccess))
            {
                return true;
            }
            else if (response == null)
            {
                return false;
            }
            else if (response.StartsWith(ActionFail))
            {
                return false;
            }
            else if (response.StartsWith(ActionSuccess))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void OnBeginEdit()
        {
            if (IsEditing)
            {
                return;
            }

            UnitNames = EnumExtensions.GetValues<Unit>()
                .Where(x => UnitNames?.FirstOrDefault(y => y.Value == x) == null)
                .Select(x => new ValueText<Unit>(x, null))
                .Union(UnitNames ?? new ValueText<Unit>[0])
                .OrderBy(x => x.Value)
                .ToArray();

            UnitModifiers = EnumExtensions.GetValues<UnitModifier>()
                .Where(x => UnitModifiers?.FirstOrDefault(y => y.Value == x) == null)
                .Select(x => new ValueText<UnitModifier>(x, null))
                .Union(UnitModifiers ?? new ValueText<UnitModifier>[0])
                .OrderBy(x => x.Value)
                .ToArray();

            FunctionCommands = EnumExtensions.GetValues<Mode>()
                .Where(x => FunctionCommands?.FirstOrDefault(y => y.Mode == x) == null)
                .Select(x => new FunctionCommandSet(x))
                .Union(FunctionCommands ?? new FunctionCommandSet[0])
                .OrderBy(x => x.Mode)
                .ToArray();

            IsEditing = true;
        }

        public void OnEndEdit()
        {
            if (!IsEditing)
            {
                return;
            }

            UnitNames = UnitNames?
                .Where(x => !string.IsNullOrEmpty(x.Text))
                .ToArray();

            UnitModifiers = UnitModifiers?
                .Where(x => !string.IsNullOrEmpty(x.Text))
                .ToArray();

            FunctionCommands = FunctionCommands?
                .Where(x => !string.IsNullOrEmpty(x.ValueCommand)
                    || !string.IsNullOrEmpty(x.RangeCommand)
                    || !string.IsNullOrEmpty(x.FunctionCommand))
                .ToArray();

            IsEditing = false;
        }
    }
}
