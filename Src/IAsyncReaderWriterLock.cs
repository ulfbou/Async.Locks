// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Async.Locks
{
    /// <summary>
    /// Represents an asynchronous reader-writer lock.
    /// </summary>
    public interface IAsyncReaderWriterLock : IAsyncLock
    {
        /// <summary>
        /// Asynchronously acquires a read lock.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
        /// <returns>A value task that represents the acquisition of the read lock. The result of the task is an <see cref="IAsyncDisposable"/> that releases the lock when disposed.</returns>
        ValueTask<IAsyncDisposable> ReadLockAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously acquires a write lock.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
        /// <returns>A value task that represents the acquisition of the write lock. The result of the task is an <see cref="IAsyncDisposable"/> that releases the lock when disposed.</returns>
        ValueTask<IAsyncDisposable> WriteLockAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously acquires an upgradeable read lock.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
        /// <returns>A value task that represents the acquisition of the upgradeable read lock. The result of the task is an <see cref="IAsyncDisposable"/> that releases the lock when disposed.</returns>
        ValueTask<IAsyncDisposable> UpgradeableReadLockAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Occurs when a read lock is acquired.
        /// </summary>
        event EventHandler<ReaderWriterLockEventArgs> ReadLockAcquired;

        /// <summary>
        /// Occurs when a write lock is acquired.
        /// </summary>
        event EventHandler<ReaderWriterLockEventArgs> WriteLockAcquired;

        /// <summary>
        /// Occurs when a read lock is released.
        /// </summary>
        event EventHandler<ReaderWriterLockEventArgs> ReadLockReleased;

        /// <summary>
        /// Occurs when a write lock is released.
        /// </summary>
        event EventHandler<ReaderWriterLockEventArgs> WriteLockReleased;
    }
}
