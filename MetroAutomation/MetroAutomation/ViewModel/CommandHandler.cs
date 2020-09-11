using System;
using System.Windows.Input;

namespace MetroAutomation.ViewModel
{
    public class CommandHandler : ICommand
    {
        private readonly Action<object> executeDelegate;

        public CommandHandler(Action executeDelegate)
        {
            this.executeDelegate = (arg) => executeDelegate();
        }

        public CommandHandler(Action<object> executeDelegate)
        {
            this.executeDelegate = executeDelegate;
        }

        public CommandHandler(Action<object> executeDelegate, Func<bool> canExecuteDelegate)
        {
            this.executeDelegate = executeDelegate;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => executeDelegate(parameter);
    }
}
