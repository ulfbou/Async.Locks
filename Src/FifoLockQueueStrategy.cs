// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Async.Locks
{
    /// <summary>
    /// A FIFO implementation of <see cref="IAsyncLockQueueStrategy"/>.
    /// </summary>
    public class FifoLockQueueStrategy : IAsyncLockQueueStrategy
    {
        private readonly ConcurrentQueue<TaskCompletionSource<IAsyncDisposable>> _queue = new();

        /// <inheritdoc />
        public void Enqueue(TaskCompletionSource<IAsyncDisposable> tcs)
        {
            lock (_queue)
            {
                _queue.Enqueue(tcs);
            }
        }

        /// <inheritdoc />
        public bool TryDequeue(out TaskCompletionSource<IAsyncDisposable>? tcs)
        {
            lock (_queue)
            {
                if (_queue.Count > 0)
                {
                    return _queue.TryDequeue(out tcs);
                }

                tcs = null;
                return false;
            }
        }
    }
}
