// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Async.Locks.Events;

namespace Async.Locks
{
    /// <summary>
    /// Represents a core implementation of an asynchronous lock.
    /// </summary>
    public sealed class AsyncLockCore : IAsyncLock, IAsyncDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        internal readonly IAsyncLockQueueStrategy<TaskCompletionSource<AsyncLockReleaser>> _queueStrategy;
        private readonly AsyncLockOptions _options;
        private int _disposed;
        private long _lockAcquisitionCount = 0;
        private long _lockReleaseCount = 0;

        /// <summary>
        /// Gets the number of times the lock has been acquired.
        /// </summary>
        public event Action? OnLockAcquired;

        /// <summary>
        /// Gets the number of times the lock has been released.
        /// </summary>
        public event Action? OnLockReleased;

        /// <summary>
        /// Occurs when the lock acquisition times out.
        /// </summary>
        public event Action? OnLockTimeout;

        /// <summary>
        /// Occurs when the lock acquisition is canceled.
        /// </summary>
        public event Action? OnLockCancelled;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLockCore"/> class.
        /// </summary>
        /// <param name="queueStrategy">The queue strategy to use for managing lock requests.</param>
        /// <param name="options">Optional configuration options for the lock.</param>
        public AsyncLockCore(IAsyncLockQueueStrategy<TaskCompletionSource<AsyncLockReleaser>> queueStrategy, AsyncLockOptions? options = null)
        {
            _semaphore = new SemaphoreSlim(1, 1);
            _queueStrategy = queueStrategy ?? new AsyncFairQueueStrategy<TaskCompletionSource<AsyncLockReleaser>>(); // Default to fair if no strategy provided
            _options = options ?? new AsyncLockOptions();
        }

        /// <summary>
        /// Gets the number of tasks waiting to acquire the lock.
        /// </summary>
        public int WaitQueueLength
        {
            get
            {
                return -1; // Indicate unavailability or implement in strategies if needed
            }
        }

        /// <summary>
        /// Gets the number of times the lock has been acquired.
        /// </summary>
        public long AcquisitionCount => Volatile.Read(ref _lockAcquisitionCount);

        /// <summary>
        /// Gets the number of times the lock has been released.
        /// </summary>
        public long ReleaseCount => Volatile.Read(ref _lockReleaseCount);

        private void InvokeLockAcquired()
        {
            OnLockAcquired?.Invoke();
            if (_options.ShouldMonitor)
            {
                AsyncLockEvents.Log.LockAcquired(Task.CurrentId ?? 0);
            }
        }

        public async ValueTask<IAsyncDisposable> AcquireAsync(CancellationToken cancellationToken = default, TimeSpan? timeout = null)
        {
            if (Volatile.Read(ref _disposed) == 1)
            {
                throw new ObjectDisposedException(nameof(AsyncLockCore));
            }
            var tcs = new TaskCompletionSource<AsyncLockReleaser>();
            _queueStrategy.Enqueue(tcs);
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var actualTimeout = timeout ?? _options.DefaultTimeout ?? Timeout.InfiniteTimeSpan;
                if (actualTimeout != Timeout.InfiniteTimeSpan && actualTimeout.TotalMilliseconds < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be greater than or equal to zero.");
                }

                if (await _semaphore.WaitAsync(actualTimeout, cancellationToken).ConfigureAwait(false))
                {
                    _queueStrategy.TryDequeue(out _); // Dequeue the current task
                    Interlocked.Increment(ref _lockAcquisitionCount);
                    InvokeLockAcquired();
                    return new AsyncLockReleaser(this);
                }
                _queueStrategy.TryDequeue(out var dequeuedTcs);
                if (dequeuedTcs == tcs)
                {
                    InvokeLockTimeout();
                    throw new TimeoutException("AsyncLock acquire timed out.");
                }
                return await tcs.Task; // Should theoretically timeout as well
            }
            catch (OperationCanceledException)
            {
                _queueStrategy.TryDequeue(out var dequeuedTcs);
                if (dequeuedTcs == tcs)
                {
                    InvokeLockCancelled();
                    throw new TaskCanceledException();
                }
                throw; // Re-throw
            }
        }
        internal void Release()
        {
            if (Volatile.Read(ref _disposed) == 1)
            {
                throw new ObjectDisposedException(nameof(AsyncLockCore));
            }

            _semaphore.Release();

            if (_queueStrategy.TryDequeue(out TaskCompletionSource<AsyncLockReleaser>? dequeuedTcs))
            {
                dequeuedTcs!.TrySetResult(new AsyncLockReleaser(this));
            }

            Interlocked.Increment(ref _lockReleaseCount);
            InvokeLockReleased();
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            {
                await _queueStrategy.DisposeAsync().ConfigureAwait(false);
                _semaphore.Dispose();
            }
        }

        private void InvokeLockReleased()
        {
            OnLockReleased?.Invoke();
            if (_options.ShouldMonitor)
            {
                AsyncLockEvents.Log.LockReleased(Task.CurrentId ?? 0);
            }
        }

        private void InvokeLockTimeout()
        {
            OnLockTimeout?.Invoke();
            if (_options.ShouldMonitor)
            {
                AsyncLockEvents.Log.LockTimeout(Task.CurrentId ?? 0);
            }
        }
        private void InvokeLockCancelled()
        {
            OnLockCancelled?.Invoke();
            if (_options.ShouldMonitor)
            {
                AsyncLockEvents.Log.LockCancelled(Task.CurrentId ?? 0);
            }
        }
    }
}
