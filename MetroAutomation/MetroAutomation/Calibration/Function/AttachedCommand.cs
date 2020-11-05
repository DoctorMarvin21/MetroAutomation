using MetroAutomation.ViewModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MetroAutomation.Calibration
{
    public enum AutoExecuteType
    {
        BeforeMode,
        AfterMode,
        AfterRange,
        AfterValue
    }

    public abstract class AttachedCommand : INotifyPropertyChanged
    {
        public AttachedCommand(Function function)
        {
            Function = function;
            ProcessCommand = new AsyncCommandHandler(() => Process(false));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Function Function { get; }

        public IAsyncCommand ProcessCommand { get; }

        public abstract AutoExecuteType AutoExecute { get; }

        public abstract Task Process(bool background);

        public abstract decimal? GetErrorArgumentValue(string argument);

        public abstract void Reset();

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
