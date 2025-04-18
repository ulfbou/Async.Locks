// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Async.Locks
{
    /// <summary>
    /// Defines a strategy for queuing asynchronous lock acquisition requests.
    /// Specialized implementations can change the queuing behavior (FIFO, LIFO, priority, etc.).
    /// </summary>
    /// typeparam name="TItem">The type of the items in the queue.</typeparam>
    public interface IAsyncLockQueueStrategy<TItem> : IAsyncDisposable where TItem : notnull
    {
        /// <summary>
        /// Gets the number of items currently in the queue.
        /// </summary>
        int WaitQueueLength { get; }

        /// <summary>
        /// Enqueues a task completion source representing a lock acquisition request.
        /// </summary>
        /// <param name="tcs">The task completion source to enqueue.</param>
        void Enqueue(TItem tcs);

        /// <summary>
        /// Attempts to dequeue a task completion source representing a lock acquisition request.
        /// </summary>
        /// <param name="tcs">When this method returns, contains the dequeued task completion source if the operation succeeded, or null if it failed.</param>
        /// <returns><c>true</c> if a task completion source was successfully dequeued; otherwise, <c>false</c>.</returns>
        bool TryDequeue(out TItem? tcs);
    }
}
