using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIB_Browser.Utilities;

public abstract class AsyncCommandBase : IAsyncCommand
{
    public abstract event EventHandler CanExecuteChanged;

    public abstract bool CanExecute(object parameter);
    public async void Execute(object parameter)
    {
        await ExecuteAsync(parameter);
    }
    public abstract Task ExecuteAsync(object parameter);
}
