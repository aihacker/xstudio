using System;
using System.Diagnostics;
using System.Windows.Input;

namespace xstudio
{
    public class CommandBase : ICommand
    {
        private readonly Func<bool> _canExecute;
        private readonly Action _command;
        private readonly Action<object> _commandpara;

        public CommandBase(Action command, Func<bool> canExecute = null)
        {
            if (command == null)
            {
                throw new ArgumentNullException();
            }
            _canExecute = canExecute;
            _command = command;
        }

        public CommandBase(Action<object> commandpara, Func<bool> canExecute = null)
        {
            if (commandpara == null)
            {
                throw new ArgumentNullException();
            }
            _canExecute = canExecute;
            _commandpara = commandpara;
        }

        public void Execute(object parameter)
        {
            if (parameter != null)
            {
                _commandpara(parameter);
            }
            else
            {
                if (_command != null)
                {
                    _command();
                }
                else if (_commandpara != null)
                {
                    _commandpara(null);
                }
            }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public event EventHandler CanExecuteChanged;
    }
}