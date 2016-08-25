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
            var sw = Stopwatch.StartNew();
            Func<object, Task> execute = async (obj) =>
            {
                Console.WriteLine($"E: {sw.Elapsed}");
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

            await Task.Delay(50);
            Assert.AreEqual(1, countBefore); //A should hav started,
            Assert.AreEqual(0, countAfter);  //But not finished.

            await Task.Delay(150);
            Assert.AreEqual(2, countBefore); //B should have started,
            Assert.AreEqual(1, countAfter);  //but not finished.

            await Task.Delay(300);
            Assert.AreEqual(2, countBefore); //Both should be finished.
            Assert.AreEqual(2, countAfter);
        }

        [Test]
        public async Task CommandCanExecuteStatus()
        {
            //Wapping some code:
            var command = new QueuedLockCommand((obj) => Task.Delay(100), _queuedLock);

            Assert.IsTrue(command.CanExecute(null));
            command.Execute(null);
            await Task.Delay(50);
            Assert.IsTrue(command.CanExecute(null));
            await Task.Delay(100);
            Assert.IsTrue(command.CanExecute(null));
        }

        [Test]
        public void CommandCanExecuteProperty()
        {
            //Wapping some code:
            var commandFalse = new QueuedLockCommand((obj) => Task.FromResult(0), (obj) => false, _queuedLock);
            var commandTrue = new QueuedLockCommand((obj) => Task.FromResult(0), (obj) => true, _queuedLock);

            Assert.IsFalse(commandFalse.CanExecute(null));
            Assert.IsTrue(commandTrue.CanExecute(null));
        }
    }
}
