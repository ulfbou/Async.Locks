// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Async.Locks
{
    /// <summary>
    /// Represents an asynchronous priority lock that allows tasks to acquire the lock based on their priority.
    /// </summary>
    /// <typeparam name="TPriority">The type of the priority used for ordering tasks.</typeparam>
    public class AsyncPriorityLock<TPriority> : IAsyncDisposable where TPriority : IComparable<TPriority>
    {
        private readonly AsyncLockCore _core;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncPriorityLock{TPriority}"/> class.
        /// </summary>
        public AsyncPriorityLock(Func<TaskCompletionSource<AsyncLockReleaser>, TPriority> prioritySelector, AsyncLockOptions? options = null)
        {
            _core = new AsyncLockCore(
                new AsyncPriorityQueueStrategy<TPriority, TaskCompletionSource<AsyncLockReleaser>>(prioritySelector),
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

        /// <summary>
        /// Acquires the lock asynchronously with a specified priority.
        /// </summary>
        /// <param name="priority">The priority of the task requesting the lock.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
        /// <param name="timeout
        /// <returns>A <see cref="ValueTask{AsyncLockReleaser}"/> that represents the acquisition of the lock. The result of the task is an <see cref="AsyncLockReleaser"/> that releases the lock when disposed.</returns>
        public ValueTask<IAsyncDisposable> AcquireAsync(TPriority priority, CancellationToken cancellationToken = default, TimeSpan? timeout = null)
        {
            var tcs = new TaskCompletionSource<AsyncLockReleaser>();
            ((AsyncPriorityQueueStrategy<TPriority, TaskCompletionSource<AsyncLockReleaser>>)_core._queueStrategy).Enqueue(tcs);
            return _core.AcquireAsync(cancellationToken, timeout);
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
