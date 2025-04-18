// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Async.Locks
{
    /// <summary>
    /// Represents options for configuring the behavior of an asynchronous lock.
    /// </summary>
    public class AsyncLockOptions
    {
        /// <summary>
        /// Gets or sets the default timeout for acquiring the lock.
        /// </summary>
        public TimeSpan? DefaultTimeout { get; set; } = Timeout.InfiniteTimeSpan;

        /// <summary>
        /// Gets or sets a value indicating whether monitoring events should be raised.
        /// </summary>
        public bool ShouldMonitor { get; set; } = false;

        /// <summary>
        /// Gets or sets the maximum size of the wait queue.
        /// </summary>
        public int MaxWaitQueueSize { get; set; } = int.MaxValue;

        /// <summary>
        /// Gets or sets a value indicating whether the lock should be fair.
        /// </summary>
        public bool IsFair { get; set; } = false;
    }
}
