using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FinanceControl.ViewModel
{
    public class ViewModelCommand : ICommand
    {
        // Поля
        private readonly Action<object> _executeAction;
        private readonly Predicate<object> _canExecuteAction;

        //Конструкторы
        public ViewModelCommand(Action<object> executeAction, Predicate<object> canExecuteAction)
        {
            _executeAction = executeAction;
            _canExecuteAction = canExecuteAction;
        }

        public ViewModelCommand(Action<object> executeAction)
        {
            _executeAction = executeAction;
        }

        //События
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }

        }


        //Методы
        public bool CanExecute(object parameter)
        {
            return _canExecuteAction == null || _canExecuteAction(parameter);
        }

        public void Execute(object parameter)
        {
            _executeAction(parameter);
        }

    }
}
