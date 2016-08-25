using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DoubleTapTest
{
    public interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync(object parameter);
    }
}
