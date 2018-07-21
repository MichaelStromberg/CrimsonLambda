using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CrimsonLauncher
{
    public static class TaskExtensions
    {
        public static void Execute<T>(this IReadOnlyList<T> items, string description,
            Action<T> executeAction)
        {
            var tasks = new Task[items.Count];

            for (var i = 0; i < items.Count; i++)
            {
                T item = items[i];
                tasks[i] = Task.Factory.StartNew(() => executeAction(item), TaskCreationOptions.LongRunning);
            }

            Task.WaitAll(tasks);
            Console.WriteLine($"- all {description} finished.\n");
        }
    }
}