using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;

namespace CrimsonLambda
{
    public static class TaskExtensions
    {
        public static void Execute<T>(this IReadOnlyList<T> items, ILambdaLogger logger, string description,
            Action<T> executeAction, int numThreads)
        {
            var tasks     = new Task[items.Count];
            var maxThread = new SemaphoreSlim(numThreads);

            for (var i = 0; i < items.Count; i++)
            {
                maxThread.Wait();
                T item = items[i];
                tasks[i] = Task.Factory.StartNew(() => executeAction(item), TaskCreationOptions.LongRunning)
                    .ContinueWith(task => maxThread.Release());
            }

            Task.WaitAll(tasks);
            logger.LogLine($"- all {description} finished.\n");
        }
    }
}