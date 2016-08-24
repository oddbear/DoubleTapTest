using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DoubleTapTest.Tests
{
    [TestFixture]
    public class WaitLockCommandTests
    {
        WaitLock _waitLock;

        [SetUp]
        public void Setup()
        {
            _waitLock = new WaitLock();
        }

        [Test]
        public async Task WaitLockTest()
        {
            //Flow: A executes, B executes, A starts, A completes, B completes
            int countBefore = 0;
            int countAfter = 0;

            Func<object, Task> execute = async (obj) =>
            {
                countBefore++;
                await Task.Delay(100);
                countAfter++;
            };

            //Execute is async void, so this will run in the background.
            new WaitLockCommand(execute, _waitLock).Execute(null);
            new WaitLockCommand(execute, _waitLock).Execute(null);
            await Task.Delay(250);

            Assert.AreEqual(1, countAfter);
            Assert.AreEqual(1, countBefore);
        }
    }
}
