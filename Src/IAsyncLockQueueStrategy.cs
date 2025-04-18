// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Async.Locks
{
    /// <summary>
    /// Defines a strategy for queuing asynchronous lock acquisition requests.
    /// Specialized implementations can change the queuing behavior (FIFO, LIFO, priority, etc.).
    /// </summary>
    public interface IAsyncLockQueueStrategy : IAsyncLockQueueStrategy<TaskCompletionSource<IAsyncDisposable>> { }
}
