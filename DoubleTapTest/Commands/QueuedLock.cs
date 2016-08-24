using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DoubleTapTest
{
    public class QueuedLock
    {
        private readonly SemaphoreSlim _semaphore;

        public bool IsLocked => _semaphore.CurrentCount == 0;

        public QueuedLock()
        {
            _semaphore = new SemaphoreSlim(1, 1);
        }

        public Task TakeLock()
        {
            return _semaphore.WaitAsync();
        }

        public void ReleaseLock()
        {
            _semaphore.Release();
        }
    }
}
