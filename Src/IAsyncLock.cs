// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Async.Locks
{
    public interface IAsyncLock : IAsyncDisposable
    {
        /// <summary>
        /// Asynchronously attempts to acquire the lock.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A task that represents the acquisition of the lock. The result of the task is a <see cref="Releaser"/> that releases the lock when disposed. If the lock is already acquired, the task result is <see langword="null"/>.</returns>
        Task<IAsyncDisposable?> TryLockAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Event that is raised when the lock is acquired.
        /// </summary>
        event EventHandler LockAcquired;

        /// <summary>
        /// Event that is raised when the lock is released.
        /// </summary>
        event EventHandler LockReleased;

        /// <summary>
        /// Gets the total number of times the lock was acquired.
        /// </summary>
        long AcquisitionCount { get; }

        /// <summary>
        /// Gets the total number of times the lock was released.
        /// </summary>
        long ReleaseCount { get; }

        /// <summary>
        /// Releases the lock.
        /// </summary>
        internal void Release();
    }
}
