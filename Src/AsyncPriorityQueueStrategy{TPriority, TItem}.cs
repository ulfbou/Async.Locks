// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;

namespace Async.Locks
{
    /// <summary>
    /// Represents a priority queue strategy for managing asynchronous lock acquisition requests.
    /// </summary>
    /// <typeparam name="TPriority">The type of the priority used for ordering tasks.</typeparam>
    /// <typeparam name="TItem">The type of the items in the queue.</typeparam>
    public class AsyncPriorityQueueStrategy<TPriority, TItem> : IAsyncLockQueueStrategy<TItem>
        where TPriority : IComparable<TPriority>
        where TItem : notnull
    {
        private readonly SortedDictionary<TPriority, Queue<TItem>> _queues = new();
        private readonly Func<TItem, TPriority> _prioritySelector;

        /// <inheritdoc />
        public int WaitQueueLength
        {
            get
            {
                lock (_queues)
                {
                    return _queues.Values.Sum(queue => queue.Count);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncPriorityQueueStrategy{TPriority, TItem}"/> class.
        /// </summary>
        /// <param name="prioritySelector">The function to select the priority for each item.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="prioritySelector"/> is <see langref="null"/>.</exception>
        public AsyncPriorityQueueStrategy(Func<TItem, TPriority> prioritySelector)
        {
            _prioritySelector = prioritySelector ?? throw new ArgumentNullException(nameof(prioritySelector));
        }

        /// <inheritdoc />
        public void Enqueue(TItem item)
        {
            var priority = _prioritySelector(item);
            lock (_queues)
            {
                if (!_queues.TryGetValue(priority, out var queue))
                {
                    queue = new Queue<TItem>();
                    _queues[priority] = queue;
                }
                queue.Enqueue(item);
            }
        }

        /// <inheritdoc />
        public bool TryDequeue(out TItem? item)
        {
            lock (_queues)
            {
                var queues = _queues.OrderByDescending(kvp => kvp.Key).ToList();
                foreach (var priorityPair in queues)
                {
                    if (priorityPair.Value.Count == 0)
                    {
                        _queues.Remove(priorityPair.Key);
                        continue;
                    }
                    var queue = _queues[priorityPair.Key];
                    if (queue?.TryDequeue(out item) == true)
                    {
                        return true;
                    }
                    Debug.WriteLine($"Failed to dequeue from priority {priorityPair.Key}");
                }
            }
            item = default;
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
