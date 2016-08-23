using System;
using System.Threading.Tasks;
using System.Windows.Input;
using FreshMvvm;

namespace DoubleTapTest
{
    public static class MyFreshExtensions
    {
        //Without parameter:
        public static ICommand CreateCommand(this IPageModelCoreMethods coreMethods, Func<Task> execute, SharedLock sharedLock = null)
        {
            return new SingleLockCommand(execute, sharedLock);
        }

        //With parameter:
        public static ICommand CreateCommand(this IPageModelCoreMethods coreMethods, Func<object, Task> execute, SharedLock sharedLock = null)
        {
            return new SingleLockCommand(execute, sharedLock);
        }

        //Use object directly: Ex. string.
        public static ICommand CreateCommand<TValue>(this IPageModelCoreMethods coreMethods, Func<TValue, Task> execute, SharedLock sharedLock = null)
        {
            return new SingleLockCommand<TValue>(execute, sharedLock);
        }

        //Convert xaml parameters from string to, ex. integer.
        public static ICommand CreateCommand<TValue>(this IPageModelCoreMethods coreMethods, Func<TValue, Task> execute, Func<string, TValue> stringConverter, SharedLock sharedLock = null)
        {
            return new SingleLockCommand<string>((obj) => execute(stringConverter(obj)), sharedLock);
        }
    }
}
