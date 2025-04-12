// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;

namespace Async.Locks
{
    /// <summary>
    /// Represents an asynchronous lock that can be used to synchronize access to a resource.
    /// </summary>
    public class AsyncLock : AsyncLockBase, IAsyncLock
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly IAsyncLockQueueStrategy _queueStrategy;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLock"/> class.
        /// </summary>
        /// <param name="queueStrategy">Optional queue strategy to use for managing lock requests.</param>
        public AsyncLock(IAsyncLockQueueStrategy? queueStrategy = null)
        {
            _semaphore = new SemaphoreSlim(1, 1);
            _queueStrategy = queueStrategy ?? new AsyncPriorityQueueStrategy<int>(tcs => 0);    // Default priority strategy (FIFO)
        }

        /// <summary>
        /// Acquires the lock asynchronously.
        /// </summary>
        /// <param name="timeout">Optional timeout for acquiring the lock.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="ValueTask{IAsyncDisposable}"/> that represents the acquisition of the lock.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the timeout is less than zero.</exception>
        /// <exception cref="TimeoutException">Thrown if the lock acquisition times out.</exception>
        /// <exception cref="TaskCanceledException">Thrown if the lock acquisition is canceled.</exception>
        protected override async ValueTask<IAsyncDisposable> AcquireInternalAsync(TimeSpan? timeout, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<IAsyncDisposable>();
            _queueStrategy.Enqueue(tcs);

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (timeout.HasValue)
                {
                    if (timeout.Value.TotalMilliseconds < 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be greater than or equal to zero.");
                    }

                    if (await _semaphore.WaitAsync(timeout.Value, cancellationToken).ConfigureAwait(false))
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

                await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                _queueStrategy.TryDequeue(out _);
                InvokeLockAcquired();
                return new AsyncLockReleaser<AsyncLock>(this);
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
