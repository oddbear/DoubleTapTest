using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DoubleTapTest.Tests
{
    [TestFixture]
    public class QueuedLockCommandTests
    {
        QueuedLock _queuedLock;

        [SetUp]
        public void Setup()
        {
            _queuedLock = new QueuedLock();
        }

        [Test]
        public async Task OnlyOneCommandWillExecuteSimultaneousTests()
        {
            //Flow: A executes, B executes, A starts, B waits, A completes, B starts, B completes.
            int countBefore = 0;
            int countAfter = 0;
            Func<object, Task> execute = async (obj) =>
            {
                Assert.AreEqual(countBefore, countAfter);
                countBefore++;
                await Task.Delay(100);
                Assert.Greater(countBefore, countAfter);
                countAfter++;
                Assert.AreEqual(countBefore, countAfter);
            };

            //Execute is async void, so this will run in the background.
            new QueuedLockCommand(execute, _queuedLock).Execute(null);
            new QueuedLockCommand(execute, _queuedLock).Execute(null);

            await Task.Delay(220); //Need to let execute finish.
            Assert.AreEqual(2, countAfter);
            Assert.AreEqual(2, countBefore);
        }

        [Test]
        public async Task CommandCanExecuteStatus()
        {
            //Wapping some code:
            var command = new QueuedLockCommand(async (obj) => await Task.Delay(100), _queuedLock);

            Assert.IsTrue(command.CanExecute(null));
            command.Execute(null);
            await Task.Delay(50);
            Assert.IsTrue(command.CanExecute(null));
            await Task.Delay(50);
            Assert.IsTrue(command.CanExecute(null));
        }
    }
}
