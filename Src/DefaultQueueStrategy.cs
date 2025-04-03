// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;

namespace Async.Locks
{
    /// <summary>
    /// Default implementation of <see cref="IAsyncLockQueueStrategy"/> that uses a simple queue.
    /// </summary>
    public class DefaultQueueStrategy : DefaultQueueStrategy<ConcurrentQueue<TaskCompletionSource<IAsyncDisposable>>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultQueueStrategy"/> class.
        /// </summary>
        public DefaultQueueStrategy() : base() { }
    }
}
