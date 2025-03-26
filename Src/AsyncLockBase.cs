// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Async.Locks
{
    /// <summary>
    /// A base class for implementing specialized asynchronous locks.
    /// </summary>
    /// <remarks>
    /// Exposes extensibility hooks for diagnostics, cancellation, timeout, and custom queuing.
    /// </remarks>
    public abstract class AsyncLockBase : IAsyncLock
    {
        protected readonly IAsyncLockQueueStrategy _queueStrategy;
        protected int _isLocked;

        public event Action? OnLockAcquired;
        public event Action? OnLockReleased;
        public event Action? OnLockTimeout;
        public event Action? OnLockCancelled;

        protected void InvokeLockAcquired() => OnLockAcquired?.Invoke();
        protected void InvokeLockReleased() => OnLockReleased?.Invoke();
        protected void InvokeLockTimeout() => OnLockTimeout?.Invoke();
        protected void InvokeLockCancelled() => OnLockCancelled?.Invoke();

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLockBase"/> class.
        /// </summary>
        /// <param name="queueStrategy">Optional queue strategy to use.</param>
        protected AsyncLockBase(IAsyncLockQueueStrategy? queueStrategy = null)
        {
            _queueStrategy = queueStrategy ?? new FifoLockQueueStrategy();
            _isLocked = 0;
        }

        /// <inheritdoc />
        public virtual async ValueTask<IAsyncDisposable> LockAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (Interlocked.CompareExchange(ref _isLocked, 1, 0) == 0)
            {
                InvokeLockAcquired();
                return new AsyncReleaser<IAsyncLock>(this);
            }

            var tcs = new TaskCompletionSource<IAsyncDisposable>(); // Removed TaskCreationOptions.RunContinuationsAsynchronously
            _queueStrategy.Enqueue(tcs);

            using var timeoutCts = timeout.HasValue ? new CancellationTokenSource(timeout.Value) : null;
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts?.Token ?? CancellationToken.None);

            CancellationTokenRegistration registration = linkedCts.Token.Register(() =>
            {
                tcs.TrySetCanceled(linkedCts.Token); // Simplified cancellation
                InvokeLockCancelled();
            }, useSynchronizationContext: false);

            try
            {
                return await tcs.Task.ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                throw new TaskCanceledException("The lock acquisition was canceled.");
            }
            finally
            {
                registration.Dispose();
            }
        }

        /// <inheritdoc />
        public virtual ValueTask ReleaseAsync()
        {
            if (_queueStrategy.TryDequeue(out var nextTcs))
            {
                InvokeLockAcquired();
                nextTcs!.SetResult(new AsyncReleaser<IAsyncLock>(this));
                InvokeLockReleased();
                return ValueTask.CompletedTask;
            }

            Interlocked.Exchange(ref _isLocked, 0);
            InvokeLockReleased();
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public virtual ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
