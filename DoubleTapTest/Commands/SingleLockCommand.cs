using System;
using System.Windows.Input;
using System.Threading.Tasks;

namespace DoubleTapTest
{
    public class SingleLockCommand : IAsyncCommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { _sharedLock.CanExecuteChanged += value; }
            remove { _sharedLock.CanExecuteChanged -= value; }
        }

        private readonly SharedLock _sharedLock;
        private readonly Func<object, Task> _execute;

        public SingleLockCommand(Func<object, Task> execute, SharedLock sharedLock)
        {
            if (execute == null)
                throw new ArgumentException(nameof(execute));
            if (sharedLock == null)
                throw new ArgumentException(nameof(sharedLock));

            _execute = execute;
            _sharedLock = sharedLock;
        }

        public SingleLockCommand(Func<Task> execute, SharedLock sharedLock)
            : this((obj) => execute(), sharedLock)
        {
            if (execute == null)
                throw new ArgumentException(nameof(execute));
        }

        public bool CanExecute(object parameter)
        {
            return !_sharedLock.IsLocked;
        }

        public void Execute(object parameter)
        {
            ExecuteAsync(parameter)
                .ContinueWith((task) =>
                {
                    //Not catchable if rethrown. Silently swallow, or kill process?
                }, TaskContinuationOptions.OnlyOnFaulted);
        }

        public async Task ExecuteAsync(object parameter)
        {
            if (_sharedLock.TakeLock()) //Ignores code block if lock already taken in SharedLock.
            {
                try
                {
                    await _execute(parameter);
                }
                finally
                {
                    _sharedLock.ReleaseLock();
                }
            }
        }
    }

    public class SingleLockCommand<TValue> : SingleLockCommand
    {
        public SingleLockCommand(Func<TValue, Task> execute, SharedLock sharedLock)
            : base(obj => execute((TValue)obj), sharedLock)
        {
            if (execute == null)
                throw new ArgumentException(nameof(execute));
        }
    }
}
