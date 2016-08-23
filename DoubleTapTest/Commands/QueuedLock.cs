using System;
using System.Threading;

namespace DoubleTapTest
{
    //TODO: Add some queue and/or callback.
    public class QueuedLock
    {
        private int _lock;

        public QueuedLock()
        {
            throw new NotImplementedException();
        }

        public bool TakeLock()
        {
            var oldVal = Interlocked.Increment(ref _lock); //Atomic increment value.
            var lockTaken = oldVal == 0;

            //Can take lock, run code...
            //Else add to a synchronous queue?

            return lockTaken;
        }

        public bool ReleaseLock()
        {
            var oldVal = Interlocked.Decrement(ref _lock); //1 goes in, 0 goes out.
            var lockReleased = oldVal == 1;

            //Do some callback stuff here? Start execution when earlier job is finished.

            return lockReleased;
        }
    }
}
