using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MIB_Browser.Utilities;

public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool> _canExecute;

    public event EventHandler CanExecuteChanged;

    public RelayCommand(Action execute, Func<bool> canExecute)
    {
        if (execute == null) throw new ArgumentNullException("execute function can't be null");
        _execute = execute;
        _canExecute = canExecute;
    }
    public RelayCommand(Action execute) : this(execute, null) { }

    public bool CanExecute(object parameter)
    {
        return _canExecute?.Invoke() ?? true;
    }

    public void Execute(object parameter)
    {
        _execute();
    }

    public void RaiseCanExecuteChanged()
    {
        var handler = CanExecuteChanged;
        if (handler is not null)
        {
            handler(this, EventArgs.Empty);
        }
    }
}
