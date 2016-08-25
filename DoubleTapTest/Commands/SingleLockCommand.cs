using System;
using System.Windows.Input;
using System.Threading.Tasks;

namespace DoubleTapTest
{
    public class SingleLockCommand : IAsyncCommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly SharedLock _sharedLock;
        private readonly Func<object, Task> _execute;

        public SingleLockCommand(Func<object, Task> execute, SharedLock sharedLock = null)
        {
            if (execute == null)
                throw new ArgumentException(nameof(execute));

            _execute = execute;
            _sharedLock = sharedLock ?? new SharedLock();
        }

        public SingleLockCommand(Func<Task> execute, SharedLock sharedLock = null)
            : this((obj) => execute(), sharedLock)
        {
            if (execute == null)
                throw new ArgumentException(nameof(execute));
        }

        public bool CanExecute(object parameter)
        {
            return !_sharedLock.IsLocked;
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public async void Execute(object parameter)
#pragma warning restore RECS0165
        {
            await ExecuteAsync(parameter);
        }

        public async Task ExecuteAsync(object parameter)
        {
            if (_sharedLock.TakeLock()) //Ignores code block if lock already taken in SharedLock.
            {
                var events = CanExecuteChanged;
                try
                {
                    if (events != null)
                        events(this, EventArgs.Empty);

                    await _execute(parameter);
                }
                finally
                {
                    _sharedLock.ReleaseLock();
                    if (events != null)
                        events(this, EventArgs.Empty);
                }
            }
        }
    }

    public class SingleLockCommand<TValue> : SingleLockCommand
    {
        public SingleLockCommand(Func<TValue, Task> execute, SharedLock sharedLock = null)
            : base(obj => execute((TValue)obj), sharedLock)
        {
            //
        }
    }
}
