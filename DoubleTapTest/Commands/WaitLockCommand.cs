using System;
using System.Windows.Input;
using System.Threading.Tasks;

namespace DoubleTapTest
{
    public class WaitLockCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly WaitLock _waitLock;
        private readonly Func<object, Task> _execute;

        public WaitLockCommand(Func<object, Task> execute, WaitLock waitLock = null)
        {
            if (execute == null)
                throw new ArgumentException(nameof(execute));

            _execute = execute;
            _waitLock = waitLock ?? new WaitLock();
        }

        public WaitLockCommand(Func<Task> execute, WaitLock queuedLock = null)
        {
            if (execute == null)
                throw new ArgumentException(nameof(execute));

            _execute = (obj) => execute();
            _waitLock = queuedLock ?? new WaitLock();
        }

        public bool CanExecute(object parameter)
        {
            return true; //It can always execute. Will then only queue new task. Correct?
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public async void Execute(object parameter)
        {
            await _waitLock.TakeLock();
            var events = CanExecuteChanged;
            try
            {
                if (events != null)
                    events(this, EventArgs.Empty);
                //Single thread here. Must have some logic for should run.

                if(_waitLock.ShouldExecute()) //If several runs this at the same time. Only the first will run, and the rest will wait for the result of this one.
                    await _execute(parameter);
                
                //TODO: Do something with some result, or execute a callback. Will need to run some code in context of this run only.
            }
            finally
            {
                if (events != null)
                    events(this, EventArgs.Empty);

                _waitLock.ReleaseLock();
            }
        }
#pragma warning restore RECS0165
    }

    public class WaitLockCommand<TValue> : WaitLockCommand
    {
        public WaitLockCommand(Func<TValue, Task> execute, WaitLock waitLock = null)
            : base(obj => execute((TValue)obj), waitLock)
        {
            //
        }
    }
}
