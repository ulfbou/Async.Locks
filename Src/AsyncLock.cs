// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Async.Locks
{
    /// <summary>
    /// Represents an asynchronous lock that can be used to synchronize access to a resource.
    /// </summary>
    public sealed class AsyncLock : IAsyncLock
    {
        private readonly AsyncLockCore _core;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLock"/> class.
        /// </summary>
        /// <param name="options">Optional configuration options for the lock.</param>
        public AsyncLock(AsyncLockOptions? options = null)
        {
            _core = new AsyncLockCore(
                options?.IsFair == true ? new AsyncFairQueueStrategy<TaskCompletionSource<AsyncLockReleaser>>() : new AsyncFairQueueStrategy<TaskCompletionSource<AsyncLockReleaser>>(),
                options);
        }

        /// <summary>
        /// Acquires the lock asynchronously.
        /// </summary>
        public int WaitQueueLength => _core.WaitQueueLength;

        /// <summary>
        /// Gets the number of times the lock has been acquired.
        /// </summary>
        public long AcquisitionCount => _core.AcquisitionCount;

        /// <summary>
        /// Gets the number of times the lock has been released.
        /// </summary>
        public long ReleaseCount => _core.ReleaseCount;

        /// <summary>
        /// Occurs when the lock is acquired.
        /// </summary>
        public event Action? OnLockAcquired
        {
            add => _core.OnLockAcquired += value;
            remove => _core.OnLockAcquired -= value;
        }

        /// <summary>
        /// Occurs when the lock is released.
        /// </summary>
        public event Action? OnLockReleased
        {
            add => _core.OnLockReleased += value;
            remove => _core.OnLockReleased -= value;
        }

        /// <summary>
        /// Occurs when the lock acquisition times out.
        /// </summary>
        public event Action? OnLockTimeout
        {
            add => _core.OnLockTimeout += value;
            remove => _core.OnLockTimeout -= value;
        }

        /// <summary>
        /// Occurs when the lock acquisition is canceled.
        /// </summary>
        public event Action? OnLockCancelled
        {
            add => _core.OnLockCancelled += value;
            remove => _core.OnLockCancelled -= value;
        }

        /// <inheritdoc />
        public ValueTask<IAsyncDisposable> AcquireAsync(CancellationToken cancellationToken = default, TimeSpan? timeout = null)
        {
            return _core.AcquireAsync(cancellationToken, timeout);
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            return _core.DisposeAsync();
        }
    }
}
