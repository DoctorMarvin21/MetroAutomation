using MetroAutomation.ViewModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MetroAutomation.Calibration
{
    public abstract class AttachedCommand : INotifyPropertyChanged
    {
        public AttachedCommand(Device device)
        {
            ProcessCommand = new AsyncCommandHandler(Process);
            Device = device;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Device Device { get; }

        public IAsyncCommand ProcessCommand { get; }

        public bool AutoExecute { get; set; }

        public abstract Task Process();

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
