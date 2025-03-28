// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Async.Locks
{
    /// <summary>
    /// Represents an asynchronous reader-writer lock that can be used to synchronize access to a resource.
    /// </summary>
    public class AsyncReaderWriterLock : AsyncLockBase
    {
        private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        /// <inheritdoc/>
        protected override async ValueTask<IAsyncDisposable> AcquireInternalAsync(TimeSpan? timeout, CancellationToken cancellationToken)
        {
            if (timeout.HasValue)
            {
                if (await Task.Run(() => _rwLock.TryEnterWriteLock(timeout.Value), cancellationToken).ConfigureAwait(false))
                {
                    InvokeLockAcquired();
                    return new AsyncLockReleaser(this, true);
                }
                else
                {
                    InvokeLockTimeout();
                    throw new TimeoutException("Lock acquisition timed out.");
                }
            }
            else
            {
                await Task.Run(() => _rwLock.EnterWriteLock(), cancellationToken).ConfigureAwait(false);
                InvokeLockAcquired();
                return new AsyncLockReleaser(this, true);
            }
        }

        /// <summary>
        /// Asynchronously acquires a read lock.
        /// </summary>
        /// <param name="timeout">Optional timeout for acquiring the lock.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="ValueTask{IAsyncDisposable}"/> that represents the acquisition of the lock. The result of the task is an <see cref="IAsyncDisposable"/> that releases the lock when disposed.</returns>
        /// 
        public async ValueTask<IAsyncDisposable> AcquireReadLockAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            if (timeout.HasValue)
            {
                if (await Task.Run(() => _rwLock.TryEnterReadLock(timeout.Value), cancellationToken).ConfigureAwait(false))
                {
                    InvokeLockAcquired();
                    return new AsyncLockReleaser(this, false);
                }
                else
                {
                    InvokeLockTimeout();
                    throw new TimeoutException("Read lock acquisition timed out.");
                }
            }
            else
            {
                await Task.Run(() => _rwLock.EnterReadLock(), cancellationToken).ConfigureAwait(false);
                InvokeLockAcquired();
                return new AsyncLockReleaser(this, false);
            }
        }

        /// <inheritdoc/>
        protected override ValueTask ReleaseInternalAsync()
        {
            throw new InvalidOperationException("Use specific release methods.");
        }

        internal void ReleaseWriteLock()
        {
            _rwLock.ExitWriteLock();
            InvokeLockReleased();
        }

        internal void ReleaseReadLock()
        {
            _rwLock.ExitReadLock();
            InvokeLockReleased();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _rwLock.Dispose();
            }
        }

        private sealed class AsyncLockReleaser : IAsyncDisposable
        {
            private readonly AsyncReaderWriterLock _lock;
            private readonly bool _isWrite;

            public AsyncLockReleaser(AsyncReaderWriterLock @lock, bool isWrite)
            {
                _lock = @lock;
                _isWrite = isWrite;
            }

            public ValueTask DisposeAsync()
            {
                if (_isWrite)
                {
                    _lock.ReleaseWriteLock();
                }
                else
                {
                    _lock.ReleaseReadLock();
                }
                return ValueTask.CompletedTask;
            }
        }
    }
}
