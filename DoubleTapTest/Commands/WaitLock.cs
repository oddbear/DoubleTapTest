using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DoubleTapTest
{
    public class WaitLock
    {
        private bool _firstRun;
        private int _lock;
        private readonly SemaphoreSlim _semaphore;

        public bool IsLocked => _semaphore.CurrentCount == 0;

        public WaitLock()
        {
            _lock = 0;
            _firstRun = true;
            _semaphore = new SemaphoreSlim(1, 1);
        }

        public Task TakeLock()
        {
            Interlocked.Increment(ref _lock); //Must be atomic.
            return _semaphore.WaitAsync();
        }

        public bool ShouldExecute()
        {
            if (!_firstRun)
                return false;
            
            _firstRun = false; //Should always run on one thread only
            return true;
        }

        public void ReleaseLock()
        {
            //Kan kanskje hente svaret også før release.
            //Interlocked.Decrement(ref _lock); //Does not need to be atomic.
            if (--_lock == 0) //Should be threadsafe because of the lock (always single thread).
                _firstRun = true; //Resets the counter
            
            _semaphore.Release();
        }
    }
}
