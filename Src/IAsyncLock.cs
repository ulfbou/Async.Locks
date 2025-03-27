// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Async.Locks
{
    /// <summary>
    /// Represents an asynchronous lock that can be used to synchronize access to a resource.
    /// </summary>
    public interface IAsyncLock : IAsyncDisposable
    {
        /// <summary>
        /// Asynchronously acquires the lock.
        /// </summary>
        /// <param name="timeout">Optional timeout for acquiring the lock.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="ValueTask"/> that represents the acquisition of the lock. The result of the task is an <see cref="IAsyncDisposable"/> that releases the lock when disposed.</returns>
        ValueTask<IAsyncDisposable> AcquireAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously releases the lock.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> that represents the asynchronous operation.</returns>
        ValueTask ReleaseAsync();
    }
}
