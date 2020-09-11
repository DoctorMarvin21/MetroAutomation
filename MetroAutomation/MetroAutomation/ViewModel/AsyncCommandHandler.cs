using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MetroAutomation.ViewModel
{
    public interface IAsyncCommand : ICommand, INotifyPropertyChanged
    {
        Task ExecuteAsync(object parameter);

        public bool IsProcessing { get; }
    }

    public class AsyncCommandHandler : IAsyncCommand, INotifyPropertyChanged
    {
        private readonly Func<object, Task> executeDelegate;
        private bool isProcessing;

        public AsyncCommandHandler(Func<Task> executeDelegate)
        {
            this.executeDelegate = (arg) => executeDelegate();
        }

        public AsyncCommandHandler(Func<object, Task> executeDelegate)
        {
            this.executeDelegate = executeDelegate;
        }

        public bool IsProcessing
        {
            get
            {
                return isProcessing;
            }
            private set
            {
                isProcessing = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsProcessing)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => !IsProcessing;

        public async void Execute(object parameter)
        {
            await ExecuteAsync(parameter);
        }

        public async Task ExecuteAsync(object parameter)
        {
            try
            {
                IsProcessing = true;
                await executeDelegate(parameter);
            }
            finally
            {
                IsProcessing = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }
}
