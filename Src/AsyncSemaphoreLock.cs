// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Async.Locks
{
    /// <summary>
    /// Represents an asynchronous semaphore lock that can be used to synchronize access to a resource.
    /// </summary>
    public class AsyncSemaphoreLock : AsyncLockBase
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        /// <inheritdoc/>
        protected override async ValueTask<IAsyncDisposable> AcquireInternalAsync(TimeSpan? timeout, CancellationToken cancellationToken)
        {
            if (timeout.HasValue)
            {
                if (await _semaphore.WaitAsync(timeout.Value, cancellationToken).ConfigureAwait(false))
                {
                    InvokeLockAcquired();
                    return new AsyncLockReleaser<AsyncSemaphoreLock>(this);
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
                InvokeLockAcquired();
                return new AsyncLockReleaser<AsyncSemaphoreLock>(this);
            }
        }

        /// <inheritdoc/>
        protected override ValueTask ReleaseInternalAsync()
        {
            _semaphore.Release();
            InvokeLockReleased();
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/> 
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _semaphore.Dispose();
            }
        }
    }
}
