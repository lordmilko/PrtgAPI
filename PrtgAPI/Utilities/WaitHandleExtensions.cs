using System;
using System.Threading;
using System.Threading.Tasks;

namespace PrtgAPI.Utilities
{
    static class WaitHandleExtensions
    {
        internal static async Task<bool> WaitOneAsync(this WaitHandle handle, int millisecondsTimeout, CancellationToken token)
        {
            RegisteredWaitHandle registeredHandle = null;

            var tokenRegistration = default(CancellationTokenRegistration);

            try
            {
                var tcs = new TaskCompletionSource<bool>();

                registeredHandle = ThreadPool.RegisterWaitForSingleObject(
                    handle,
                    (s, t) => {
                        var val = (Tuple<TaskCompletionSource<bool>, CancellationToken>)s;

                        //In Release builds, there is a race condition between the CancellationTokenSource
                        //callback and the ManualResetToken.Set() callback. To mitigate this, if the
                        //waithandle Set() callback is called first, we also check whether we need to perform
                        //the work of the CancellationTokenSource callback. When the CancellationTokenSource callback
                        //is finally called, it will return false as it always does, due to TrySetResult already
                        //having been called.
                        if (val.Item2.IsCancellationRequested)
                            val.Item1.TrySetCanceled();

                            val.Item1.TrySetResult(!t);
                        },
                    Tuple.Create(tcs, token),
                    millisecondsTimeout,
                    true
                );

                tokenRegistration = token.Register(
                    s => ((TaskCompletionSource<bool>)s).TrySetCanceled(),
                    tcs
                );

                return await tcs.Task.ConfigureAwait(false);
            }
            finally
            {
                registeredHandle?.Unregister(null);

                tokenRegistration.Dispose();
            }
        }
    }
}
