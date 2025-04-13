﻿// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Async.Locks.Events;

namespace Async.Locks
{
    public abstract class AsyncLockBase : IAsyncLock, IDisposable
    {
        private int _disposed;
        private readonly bool _shouldMonitor;

        /// <summary>
        /// Occurs when the lock is acquired.
        /// </summary>
        public event Action? OnLockAcquired;

        /// <summary>
        /// Occurs when the lock is released.
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
        /// Initializes a new instance of the <see cref="AsyncLockBase"/> class.
        /// </summary>
        /// <param name="shouldMonitor">Optional parameter indicating whether monitoring events should be raised. Defaults to <c>false</c>.</param>
        protected AsyncLockBase(bool shouldMonitor = false)
        {
            _shouldMonitor = shouldMonitor;
        }

        /// <summary>
        /// Invokes the <see cref="OnLockAcquired"/> event and optionally logs a monitoring event.
        /// </summary>
        protected void InvokeLockAcquired()
        {
            OnLockAcquired?.Invoke();
            if (_shouldMonitor)
            {
                AsyncLockEvents.Log.LockAcquired(Task.CurrentId ?? 0);
            }
        }

        /// <summary>
        /// Invokes the <see cref="OnLockReleased"/> event and optionally logs a monitoring event.
        /// </summary>
        protected void InvokeLockReleased()
        {
            OnLockReleased?.Invoke();
            if (_shouldMonitor)
            {
                AsyncLockEvents.Log.LockReleased(Task.CurrentId ?? 0);
            }
        }

        /// <summary>
        /// Invokes the <see cref="OnLockTimeout"/> event and optionally logs a monitoring event.
        /// </summary>
        protected void InvokeLockTimeout()
        {
            OnLockTimeout?.Invoke();
            if (_shouldMonitor)
            {
                AsyncLockEvents.Log.LockTimeout(Task.CurrentId ?? 0);
            }
        }

        /// <summary>
        /// Invokes the <see cref="OnLockCancelled"/> event and optionally logs a monitoring event.
        /// </summary>
        protected void InvokeLockCancelled()
        {
            OnLockCancelled?.Invoke();
            if (_shouldMonitor)
            {
                AsyncLockEvents.Log.LockCancelled(Task.CurrentId ?? 0);
            }
        }

        /// <summary>
        /// Asynchronously acquires the lock.
        /// </summary>
        /// <param name="timeout">Optional timeout for acquiring the lock.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="ValueTask{IAsyncDisposable}"/> that represents the acquisition of the lock. The result of the task is an <see cref="IAsyncDisposable"/> that releases the lock when disposed.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the lock has been disposed.</exception>
        public async ValueTask<IAsyncDisposable> AcquireAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            if (Volatile.Read(ref _disposed) == 1)
            {
                throw new ObjectDisposedException(nameof(AsyncLockBase));
            }

            return await AcquireInternalAsync(timeout, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Acquires the lock asynchronously.
        /// </summary>
        /// <param name="timeout">Optional timeout for acquiring the lock.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="ValueTask{IAsyncDisposable}"/> that represents the acquisition of the lock.</returns>
        protected abstract ValueTask<IAsyncDisposable> AcquireInternalAsync(TimeSpan? timeout, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously releases the lock.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> that represents the asynchronous operation.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the lock has been disposed.</exception>
        public ValueTask ReleaseAsync()
        {
            if (Volatile.Read(ref _disposed) == 1)
            {
                throw new ObjectDisposedException(nameof(AsyncLockBase));
            }

            return ReleaseInternalAsync();
        }

        /// <summary>
        /// Releases the lock asynchronously.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> that represents the asynchronous operation.</returns>
        protected abstract ValueTask ReleaseInternalAsync();

        /// <summary>
        /// Disposes the lock asynchronously.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> that represents the asynchronous operation.</returns>
        public virtual async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            {
                Interlocked.Increment(ref _disposed);
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            await Task.Yield();
        }

        /// <summary>
        /// Disposes the lock.
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            {
                Interlocked.Increment(ref _disposed);
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Disposes the lock.
        /// </summary>
        /// <param name="disposing">A boolean value indicating whether the method is being called from the Dispose method.</param>
        protected abstract void Dispose(bool disposing);

        /// <summary>
        /// Finalizes the lock.
        /// </summary>
        ~AsyncLockBase()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            {
                Interlocked.Increment(ref _disposed);
                Dispose(disposing: false);
            }
        }
    }
}
