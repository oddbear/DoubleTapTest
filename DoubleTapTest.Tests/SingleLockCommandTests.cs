using NUnit.Framework;
using System;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DoubleTapTest.Tests
{
    [TestFixture]
    public class SingleLockCommandTests
    {
        SharedLock _sharedLock;

        [SetUp]
        public void Setup()
        {
            _sharedLock = new SharedLock();
        }

        [Test]
        public async Task OnlyOneCommandWillExecuteTests()
        {
            //Flow: A executes, B executes, A starts, B ignored, A completes.
            int countBefore = 0;
            int countAfter = 0;
            Func<object, Task> execute = async (obj) => {
                countBefore++;
                await Task.Delay(100);
                countAfter++;
            };

            //Execute is async void, so this will run in the background.
            new SingleLockCommand(execute, _sharedLock).Execute(null);
            new SingleLockCommand(execute, _sharedLock).Execute(null);

            await Task.Delay(250); //Need to let execute finish.

            Assert.AreEqual(1, countBefore);
            Assert.AreEqual(1, countAfter);
        }

        [Test]
        public async Task CommandCanExecuteStatus()
        {
            //Wapping some code:
            var command = new SingleLockCommand(async (obj) => await Task.Delay(100), _sharedLock);

            Assert.IsTrue(command.CanExecute(null));
            command.Execute(null);
            await Task.Delay(50);
            Assert.IsFalse(command.CanExecute(null));
            await Task.Delay(50);
            Assert.IsTrue(command.CanExecute(null));
        }
    }
}
