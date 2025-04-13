// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Async.Locks
{
    /// <summary>
    /// Represents an asynchronous lock that can be used to synchronize access to a resource.
    /// </summary>
    public class AsyncLock : AsyncLockBase, IAsyncLock
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly TimeSpan _defaultTimeout;
        private readonly IAsyncLockQueueStrategy _queueStrategy;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLock"/> class.
        /// </summary>
        /// <param name="queueStrategy">Optional queue strategy to use for managing lock requests. Defaults to FIFO.</param>
        /// <param name="defaultTimeout">Optional default timeout to use when acquiring the lock without specifying a timeout. Defaults to <see cref="Timeout.InfiniteTimeSpan"/>.</param>
        /// <param name="shouldMonitor">Optional parameter indicating whether monitoring events should be raised. Defaults to <c>false</c>.</param>
        public AsyncLock(IAsyncLockQueueStrategy? queueStrategy = null, TimeSpan? defaultTimeout = null, bool shouldMonitor = false) : base(shouldMonitor)
        {
            _semaphore = new SemaphoreSlim(1, 1);
            _defaultTimeout = defaultTimeout ?? Timeout.InfiniteTimeSpan;
            _queueStrategy = queueStrategy ?? new AsyncPriorityQueueStrategy<int>(tcs => 0); // Default priority strategy (FIFO)
        }

        /// <summary>
        /// Acquires the lock asynchronously.
        /// </summary>
        /// <param name="timeout">Optional timeout for acquiring the lock. If <c>null</c>, the <see cref="defaultTimeout"/> specified during construction will be used. To wait indefinitely, use <see cref="Timeout.InfiniteTimeSpan"/> or pass <c>null</c> when no <c>defaultTimeout</c> was provided.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="ValueTask{IAsyncDisposable}"/> that represents the acquisition of the lock. The result of the task is an <see cref="IAsyncDisposable"/> that releases the lock when disposed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the provided timeout is less than zero.</exception>
        /// <exception cref="TimeoutException">Thrown if the lock acquisition times out.</exception>
        /// <exception cref="TaskCanceledException">Thrown if the lock acquisition is canceled.</exception>
        protected override async ValueTask<IAsyncDisposable> AcquireInternalAsync(TimeSpan? timeout, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<IAsyncDisposable>();
            _queueStrategy.Enqueue(tcs);

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var actualTimeout = timeout ?? _defaultTimeout;

                if (actualTimeout != Timeout.InfiniteTimeSpan && actualTimeout.TotalMilliseconds < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be greater than or equal to zero.");
                }

                if (await _semaphore.WaitAsync(actualTimeout, cancellationToken).ConfigureAwait(false))
                {
                    _queueStrategy.TryDequeue(out _);
                    InvokeLockAcquired();
                    return new AsyncLockReleaser<AsyncLock>(this);
                }

                _queueStrategy.TryDequeue(out var dequeuedTcs);

                if (dequeuedTcs != null && dequeuedTcs == tcs)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    InvokeLockTimeout();
                    throw new TimeoutException("AsyncLock acquire timed out.");
                }

                return await tcs.Task;
            }
            catch (OperationCanceledException)
            {
                _queueStrategy.TryDequeue(out var dequeuedTcs);

                if (dequeuedTcs != null && dequeuedTcs == tcs)
                {
                    InvokeLockCancelled();
                    throw new TaskCanceledException();
                }

                return await tcs.Task;
            }
        }

        /// <summary>
        /// Releases the lock asynchronously.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> that represents the asynchronous operation.</returns>
        protected override ValueTask ReleaseInternalAsync()
        {
            _semaphore.Release();

            if (_queueStrategy.TryDequeue(out var nextTcs))
            {
                nextTcs!.SetResult(new AsyncLockReleaser<AsyncLock>(this));
            }

            InvokeLockReleased();
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Disposes the resources used by the <see cref="AsyncLock"/> class.
        /// </summary>
        /// <param name="disposing">A boolean value indicating whether the method is being called from the Dispose method.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _semaphore?.Dispose();
            }
        }
    }
}
