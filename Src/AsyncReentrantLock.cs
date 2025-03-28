// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Async.Locks
{
    /// <summary>
    /// Represents an asynchronous reentrant lock that can be used to synchronize access to a resource.
    /// </summary>
    public class AsyncReentrantLock : AsyncLockBase
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly ThreadLocal<int> _recursionCount = new ThreadLocal<int>();
        private readonly ThreadLocal<int> _ownerThreadId = new ThreadLocal<int>();

        /// <inheritdoc/>
        protected override async ValueTask<IAsyncDisposable> AcquireInternalAsync(TimeSpan? timeout, CancellationToken cancellationToken)
        {
            var currentThreadId = Environment.CurrentManagedThreadId;

            if (_ownerThreadId.IsValueCreated && _ownerThreadId.Value == currentThreadId)
            {
                _recursionCount.Value++;
                InvokeLockAcquired();
                return new AsyncLockReleaser<AsyncReentrantLock>(this);
            }

            if (timeout.HasValue)
            {
                if (await _semaphore.WaitAsync(timeout.Value, cancellationToken).ConfigureAwait(false))
                {
                    _ownerThreadId.Value = currentThreadId;
                    _recursionCount.Value = 1;
                    InvokeLockAcquired();
                    return new AsyncLockReleaser<AsyncReentrantLock>(this);
                }
                else
                {
                    InvokeLockTimeout();
                    throw new TimeoutException("Lock acquisition timed out.");
                }
            }
            else
            {
                await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                _ownerThreadId.Value = currentThreadId;
                _recursionCount.Value = 1;
                InvokeLockAcquired();
                return new AsyncLockReleaser<AsyncReentrantLock>(this);
            }
        }

        /// <inheritdoc/>
        protected override ValueTask ReleaseInternalAsync()
        {
            if (_ownerThreadId.Value != Environment.CurrentManagedThreadId)
            {
                throw new InvalidOperationException("Lock released by wrong thread");
            }

            if (_recursionCount.Value > 1)
            {
                _recursionCount.Value--;
            }
            else
            {
                _recursionCount.Value = 0;
                _ownerThreadId.Value = 0;
                _semaphore.Release();
                InvokeLockReleased();
            }
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _semaphore.Dispose();
                _recursionCount.Dispose();
                _ownerThreadId.Dispose();
            }
        }
    }
}
