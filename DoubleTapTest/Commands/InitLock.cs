using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DoubleTapTest
{
    public class InitLock<TResult>
    {
        private int _queueLength;
        private TResult _result;

        private readonly SemaphoreSlim _semaphore;
        private readonly Func<Task<TResult>> _initializer;

        public bool IsLocked => _semaphore.CurrentCount == 0;

        public InitLock(Func<Task<TResult>> initializer)
        {
            if (initializer == null)
                throw new ArgumentException(nameof(initializer));
            
            _initializer = initializer;

            _result = default(TResult);
            _queueLength = 0;
            _semaphore = new SemaphoreSlim(1, 1);
        }

        public async Task<TResult> TryInitializie()
        {
            Interlocked.Increment(ref _queueLength); //Must be atomic, and before WaitAsync();
            await _semaphore.WaitAsync(); //Queues up.
            try
            {
                if (_queueLength == 1) //If first.
                    _result = await _initializer();
            }
            finally
            {
                _semaphore.Release();
            }
            return _result;
        }

        public void FinishedExecuting()
        {
            Interlocked.Decrement(ref _queueLength); //Cound down for new initialization (resets when hitting 0). Can later add some cache, or manual refresh.
        }
    }
}
