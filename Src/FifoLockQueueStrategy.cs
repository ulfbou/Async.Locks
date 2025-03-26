// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Async.Locks
{
    /// <summary>
    /// A basic FIFO implementation of IAsyncLockQueueStrategy.
    /// </summary>
    public class FifoLockQueueStrategy : IAsyncLockQueueStrategy
    {
        private readonly Queue<TaskCompletionSource<IAsyncDisposable>> _queue = new();

        /// <inheritdoc />
        public void Enqueue(TaskCompletionSource<IAsyncDisposable> tcs)
        {
            _queue.Enqueue(tcs);
        }

        /// <inheritdoc />
        public bool TryDequeue(out TaskCompletionSource<IAsyncDisposable>? tcs)
        {
            if (_queue.Count > 0)
            {
                tcs = _queue.Dequeue();
                return true;
            }
            tcs = null;
            return false;
        }
    }
}
