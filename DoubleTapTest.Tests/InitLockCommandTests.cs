using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DoubleTapTest.Tests
{
    [TestFixture]
    public class InitLockCommandTests
    {
        [Test]
        public async Task InitLockTest()
        {
            //Flow: A initialize executes, B initialize waits, A done initializing, B skips initializing, A executes, B executes, A completes, B completes, then resets.
            int initializeCountBefore = 0;
            int initializeCountAfter = 0;

            int countBefore = 0;
            int countAfter = 0;

            var response = new object();
            var sw = Stopwatch.StartNew();
            Func<Task<object>> initialize = async () =>
            {
                initializeCountBefore++;
                await Task.Delay(100);
                initializeCountAfter++;
                return response;
            };

            var initLock = new InitLock<object>(initialize);

            Func<object, Task> execute = async (obj) =>
            {
                Assert.AreSame(response, obj);
                countBefore++;
                await Task.Delay(100);
                countAfter++;
            };

            //Execute is async void, so this will run in the background.
            new InitLockCommand<object>(execute, initLock).Execute(null);
            new InitLockCommand<object>(execute, initLock).Execute(null);

            await Task.Delay(50);
            Assert.AreEqual(1, initializeCountBefore);
            Assert.AreEqual(0, initializeCountAfter);

            await Task.Delay(100);
            Assert.AreEqual(1, initializeCountBefore);
            Assert.AreEqual(1, initializeCountAfter);

            Assert.AreEqual(2, countBefore);
            Assert.AreEqual(0, countAfter);

            await Task.Delay(100);
            Assert.AreEqual(2, countBefore);
            Assert.AreEqual(2, countAfter);

            new InitLockCommand<object>(execute, initLock).Execute(null);
            await Task.Delay(50);
            Assert.AreEqual(2, initializeCountBefore);
        }
    }
}
