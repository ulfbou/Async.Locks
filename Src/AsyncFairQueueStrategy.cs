// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;

namespace Async.Locks
{
    /// <summary>
    /// Represents a fair queue strategy for managing asynchronous lock acquisition requests.
    /// </summary>
    /// <typeparam name="T">The type of the items in the queue.</typeparam>
    public class AsyncFairQueueStrategy<T> : IAsyncLockQueueStrategy<T> where T : notnull
    {
        private readonly ConcurrentQueue<T> _queue = new();

        /// <inheritdoc />
        public void Enqueue(T item)
        {
            _queue.Enqueue(item);
        }

        /// <inheritdoc />
        public bool TryDequeue(out T? item)
        {
            return _queue.TryDequeue(out item);
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}
