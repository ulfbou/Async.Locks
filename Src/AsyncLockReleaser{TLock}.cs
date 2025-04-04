﻿// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Async.Locks
{
    /// <summary>
    /// Represents a releaser that releases the lock when disposed.
    /// </summary>
    public readonly struct AsyncLockReleaser<TLock> : IAsyncDisposable where TLock : IAsyncLock
    {
        private readonly TLock _asyncLock;

        /// <summary>
        /// Initializes a new instance of the <see langword="readonly"/> <see cref="AsyncLockReleaser{TLock}"/> structure.
        /// </summary>
        /// <param name="asyncLock">The <see cref="IAsyncLock"/> to release.</param>
        internal AsyncLockReleaser(TLock asyncLock)
        {
            _asyncLock = asyncLock ?? throw new ArgumentNullException(nameof(asyncLock));
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            _asyncLock.ReleaseAsync();
            return ValueTask.CompletedTask;
        }
    }
}
