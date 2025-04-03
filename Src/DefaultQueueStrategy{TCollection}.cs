// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Async.Locks
{
    /// <summary>
    /// Generic default implementation of <see cref="IAsyncLockQueueStrategy"/> that uses a generic collection for a queue.
    /// </summary>
    /// <typeparam name="TCollection">The type of the collection to use for the queue.</typeparam>
    public class DefaultQueueStrategy<TCollection> : IAsyncLockQueueStrategy where TCollection : IProducerConsumerCollection<TaskCompletionSource<IAsyncDisposable>>, new()
    {
        private readonly ConcurrentDictionary<int, TCollection> _queues = new();

        /// <inheritdoc />
        public void Enqueue(TaskCompletionSource<IAsyncDisposable> tcs, int priority = 0)
        {
            lock (_queues)
            {
                TCollection queue = _queues.GetOrAdd(priority, _ => new TCollection());
                queue.TryAdd(tcs);
            }
        }

        /// <inheritdoc />
        public bool TryDequeue([MaybeNullWhen(false)] out TaskCompletionSource<IAsyncDisposable> tcs)
        {
            lock (_queues)
            {
                if (_queues.Count > 0)
                {
                    IEnumerable<KeyValuePair<int, TCollection>> queues = _queues.OrderByDescending(kvp => kvp.Key);

                    while (queues.Any())
                    {
                        var priority = queues.First().Key;
                        queues = queues.Skip(1).AsEnumerable();
                        var queue = _queues[priority];

                        if (queue.Count > 0 && queue.TryTake(out tcs))
                        {
                            if (queue.Count == 0)
                            {
                                _queues.TryRemove(priority, out _);
                            }

                            return true;
                        }

                        _queues.TryRemove(priority, out _);
                    }
                }
            }

            tcs = null;
            return false;
        }
    }
}
