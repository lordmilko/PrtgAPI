using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PrtgAPI.Helpers
{
    static class TaskHelpers
    {
        //https://blogs.msdn.microsoft.com/pfxteam/2012/08/02/processing-tasks-as-they-complete/
        public static Task<Task<T>>[] WhenAnyForAll<T>(this IEnumerable<Task<T>> tasks)
        {
            var inputTasks = tasks.ToList();

            var buckets = new TaskCompletionSource<Task<T>>[inputTasks.Count];

            var results = new Task<Task<T>>[buckets.Length];

            for (int i = 0; i < buckets.Length; i++)
            {
                buckets[i] = new TaskCompletionSource<Task<T>>();
                results[i] = buckets[i].Task;
            }

            int nextTaskIndex = -1;

            Action<Task<T>> continuation = completed =>
            {
                var bucket = buckets[Interlocked.Increment(ref nextTaskIndex)];
                bucket.TrySetResult(completed);
            };

            foreach (var inputTask in inputTasks)
                inputTask.ContinueWith(continuation, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

            return results;
        }

        public static async void WhenAnyForAll2<T>(IEnumerable<Task<T>> tasks)
        {
            //var generator = new ObjectGenerator2(tasks.WhenAnyForAll());

            foreach (var item in (tasks).WhenAnyForAll())
            {
                var t = await item;
                var result = await t;

            }
        }
    }
}
