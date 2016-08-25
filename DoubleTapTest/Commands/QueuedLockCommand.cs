using System;
using System.Windows.Input;
using System.Threading.Tasks;

namespace DoubleTapTest
{
    public class QueuedLockCommand : IAsyncCommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly QueuedLock _queuedLock;
        private readonly Func<object, Task> _execute;
        private readonly Func<object, bool> _canExecute;

        public QueuedLockCommand(Func<Task> execute, QueuedLock queuedLock = null)
            : this(execute, null, queuedLock) { /* ... */ }
        
        public QueuedLockCommand(Func<object, Task> execute, QueuedLock queuedLock = null)
            : this(execute, null, queuedLock) { /* ... */ }
        
        public QueuedLockCommand(Func<Task> execute, Func<object, bool> canExecute, QueuedLock queuedLock = null)
            : this((obj) => execute(), canExecute, queuedLock)
        {
            if (execute == null)
                throw new ArgumentException(nameof(execute));
        }

        public QueuedLockCommand(Func<object, Task> execute, Func<object, bool> canExecute, QueuedLock queuedLock = null)
        {
            if (execute == null)
                throw new ArgumentException(nameof(execute));

            _execute = execute;
            _canExecute = canExecute;
            _queuedLock = queuedLock ?? new QueuedLock();
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
                return true;
            
            return _canExecute(parameter);
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public async void Execute(object parameter)
#pragma warning restore RECS0165
        {
            await ExecuteAsync(parameter);
        }

        public async Task ExecuteAsync(object parameter)
        {
            await _queuedLock.TakeLock();
            var events = CanExecuteChanged;
            try
            {
                if (events != null)
                    events(this, EventArgs.Empty);

                await _execute(parameter);
            }
            finally
            {
                if (events != null)
                    events(this, EventArgs.Empty);

                _queuedLock.ReleaseLock();
            }
        }
    }

    public class QueuedLockCommand<TValue> : QueuedLockCommand
    {
        public QueuedLockCommand(Func<TValue, Task> execute, QueuedLock queuedLock = null)
            : base(obj => execute((TValue)obj), queuedLock)
        {
            //
        }

        public QueuedLockCommand(Func<TValue, Task> execute, Func<TValue, bool> canExecute, QueuedLock queuedLock = null)
            : base(obj => execute((TValue)obj), (obj) => canExecute((TValue)obj), queuedLock)
        {
            //
        }
    }
}
