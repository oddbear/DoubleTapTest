using System;
using System.Windows.Input;
using System.Threading.Tasks;

namespace DoubleTapTest
{
    public class QueuedLockCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly QueuedLock _queuedLock;
        private readonly Func<object, Task> _execute;

        public QueuedLockCommand(Func<object, Task> execute, QueuedLock queuedLock = null)
        {
            if (execute == null)
                throw new ArgumentException(nameof(execute));

            _execute = execute;
            _queuedLock = queuedLock ?? new QueuedLock();
        }

        public QueuedLockCommand(Func<Task> execute, QueuedLock queuedLock = null)
        {
            if (execute == null)
                throw new ArgumentException(nameof(execute));

            _execute = (obj) => execute();
            _queuedLock = queuedLock ?? new QueuedLock();
        }

        public bool CanExecute(object parameter)
        {
            return true; //It can always execute. Will then only queue new task. Correct?
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public async void Execute(object parameter)
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
#pragma warning restore RECS0165
    }

    public class QueuedLockCommand<TValue> : QueuedLockCommand
    {
        public QueuedLockCommand(Func<TValue, Task> execute, QueuedLock queuedLock = null)
            : base(obj => execute((TValue)obj), queuedLock)
        {
            //
        }
    }
}
