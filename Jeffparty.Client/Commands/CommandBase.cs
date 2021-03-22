using System;
using System.Windows.Input;

namespace Jeffparty.Client.Commands
{
    public abstract class CommandBase : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public void NotifyExecutabilityChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public virtual bool CanExecute(object? parameter)
        {
            throw new NotImplementedException();
        }

        public virtual void Execute(object? parameter)
        {
            throw new NotImplementedException();
        }
    }
}