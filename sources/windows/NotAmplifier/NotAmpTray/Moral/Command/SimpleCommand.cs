using System;
using System.Windows.Input;

namespace Moral.Command
{
    abstract class SimpleCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        abstract public void Execute(object parameter);
    }
}
