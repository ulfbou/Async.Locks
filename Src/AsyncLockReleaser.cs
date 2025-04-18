// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Async.Locks
{
    /// <summary>
    /// Represents a releaser that releases the lock when disposed.
    /// </summary>
    public readonly struct AsyncLockReleaser : IAsyncDisposable
    {
        private readonly AsyncLockCore _asyncLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLockReleaser"/> struct.
        /// </summary>
        /// <param name="asyncLock">The asynchronous lock to release.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="asyncLock"/> is null.</exception>
        internal AsyncLockReleaser(AsyncLockCore asyncLock)
        {
            _asyncLock = asyncLock ?? throw new ArgumentNullException(nameof(asyncLock));
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            _asyncLock.Release();
            return ValueTask.CompletedTask;
        }
    }
}
