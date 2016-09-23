using System;
using System.Threading;

namespace DoubleTapTest
{
    public class SharedLock
    {
        private int _lock;

        private readonly Guid LogId;

        public bool IsLocked => _lock != 0;

        public event EventHandler CanExecuteChanged;

        public SharedLock()
        {
            _lock = 0;
            LogId = Guid.NewGuid();
            System.Diagnostics.Debug.WriteLine($"Lock created: {LogId}");
        }

        public bool TakeLock()
        {
            var oldVal = Interlocked.Exchange(ref _lock, 1); //Atomic swap values.
            var lockTaken = oldVal == 0;

            if (lockTaken)
                System.Diagnostics.Debug.WriteLine($"Lock taken: {LogId}");
            else
                System.Diagnostics.Debug.WriteLine($"Lock not taken: {LogId}");

            var events = CanExecuteChanged;
            if(lockTaken && events != null)
                events(this, EventArgs.Empty);
            
            return lockTaken;
        }

        public bool ReleaseLock()
        {
            var oldVal = Interlocked.Exchange(ref _lock, 0);
            var lockReleased = oldVal == 1;

            if (lockReleased)
                System.Diagnostics.Debug.WriteLine($"Lock released: {LogId}");
            else
                throw new InvalidOperationException($"Lock not released: {LogId}"); //Should not be possible.

            var events = CanExecuteChanged;
            if (lockReleased && events != null)
                events(this, EventArgs.Empty);
            
            return lockReleased;
        }
    }
}
