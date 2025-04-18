// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;

namespace Async.Locks
{
    /// <summary>
    /// Represents a priority queue strategy for managing asynchronous lock acquisition requests.
    /// This strategy allows for prioritizing lock requests based on a specified priority.
    /// </summary>
    /// typeparam name="TPriority">The type of the priority used for ordering tasks.</typeparam>
    public class AsyncPriorityQueueStrategy<TPriority> : IAsyncLockQueueStrategy
        where TPriority : IComparable<TPriority>
    {
        private readonly SortedDictionary<TPriority, Queue<TaskCompletionSource<IAsyncDisposable>>> _queues = new();

        /// <summary>
        /// Gets the function used to select the priority for each item.
        /// </summary>
        /// <remarks>
        /// This function takes a <see cref="TaskCompletionSource{IAsyncDisposable}"/> and returns the priority for that item.
        /// </remarks>
        public Func<TaskCompletionSource<IAsyncDisposable>, TPriority> PrioritySelector { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncPriorityQueueStrategy{TPriority}"/> class.
        /// </summary>
        /// <param name="prioritySelector">The function to select the priority for each item.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="prioritySelector"/> is <see langref="null"/>.</exception>
        public AsyncPriorityQueueStrategy(Func<TaskCompletionSource<IAsyncDisposable>, TPriority> prioritySelector)
        {
            PrioritySelector = prioritySelector ?? throw new ArgumentNullException(nameof(prioritySelector));
        }

        /// <inheritdoc />
        public void Enqueue(TaskCompletionSource<IAsyncDisposable> tcs)
        {
            var priority = PrioritySelector(tcs);

            lock (_queues)
            {
                if (!_queues.TryGetValue(priority, out var queue))
                {
                    queue = new Queue<TaskCompletionSource<IAsyncDisposable>>();
                    _queues[priority] = queue;
                }

                queue.Enqueue(tcs);
            }
        }

        /// <inheritdoc />
        public bool TryDequeue(out TaskCompletionSource<IAsyncDisposable>? tcs)
        {
            lock (_queues)
            {
                var queues = _queues.OrderByDescending(kvp => kvp.Key).ToList();

                foreach (var priorityPair in queues)
                {
                    if (priorityPair.Value.Count == 0)
                    {
                        // Remove the queue with the highest priority.
                        _queues.Remove(priorityPair.Key);
                        continue;
                    }

                    // Attempt to dequeue from the queue with currently highest priority.
                    var queue = _queues[priorityPair.Key];

                    if (queue?.TryDequeue(out tcs) == true)
                    {
                        return true;
                    }

                    // Log the failure to dequeue.
                    Debug.WriteLine($"Failed to dequeue from priority {priorityPair.Key}");
                }
            }

            tcs = default!;
            return false;
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            lock (_queues)
            {
                _queues.Clear();
            }

            return ValueTask.CompletedTask;
        }
    }
}
