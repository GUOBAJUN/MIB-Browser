using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIB_Browser.Utilities;

public class AsyncRelayCommand : AsyncCommandBase
{
    private readonly Func<Task> _execute;
    private readonly Func<bool> _canExecute;

    public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute)
    {
        _execute = execute ?? throw new ArgumentNullException("execute can't be null");
        _canExecute = canExecute;
    }

    public override event EventHandler CanExecuteChanged;

    public override bool CanExecute(object parameter)
    {
        return _canExecute?.Invoke() ?? true;
    }

    public override Task ExecuteAsync(object parameter)
    {
        return _execute();
    }
}
