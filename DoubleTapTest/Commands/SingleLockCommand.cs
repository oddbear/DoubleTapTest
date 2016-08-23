using System;
using System.Windows.Input;
using System.Threading.Tasks;

namespace DoubleTapTest
{
    public class SingleLockCommand : ICommand
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
        {
            if (execute == null)
                throw new ArgumentException(nameof(execute));

            _execute = (obj) => execute();
            _sharedLock = sharedLock ?? new SharedLock();
        }

        public bool CanExecute(object parameter)
        {
            return !_sharedLock.IsLocked;
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public async void Execute(object parameter)
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
                //TODO: Should I have som exception handling here?
                finally
                {
                    _sharedLock.ReleaseLock();
                    if (events != null)
                        events(this, EventArgs.Empty);
                }
            }
        }
#pragma warning restore RECS0165
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
