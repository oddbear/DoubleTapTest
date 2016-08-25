using System;
using System.Windows.Input;
using System.Threading.Tasks;

namespace DoubleTapTest
{
    public class InitLockCommand<TResult> : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly InitLock<TResult> _initLock;
        private readonly Func<TResult, Task> _execute;

        public InitLockCommand(Func<TResult, Task> execute, InitLock<TResult> initLock)
        {
            
            if (execute == null)
                throw new ArgumentException(nameof(execute));
            
            if (initLock == null)
                throw new ArgumentException(nameof(initLock));
            
            _execute = execute;
            _initLock = initLock;
        }

        public bool CanExecute(object parameter)
        {
            return true; //It can always execute. Will then only queue new task. Correct?
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public async void Execute(object parameter)
#pragma warning restore RECS0165
        {
            await ExecuteAsync();
        }

        public async Task ExecuteAsync()
        {
            var result = await _initLock.TryInitializie();
            try
            {
                await _execute(result); //Executes with shared result from first run.
            }
            finally
            {
                _initLock.FinishedExecuting();
            }
        }
    }
}
