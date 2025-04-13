// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Async.Locks
{
    /// <summary>
    /// A LIFO implementation of <see cref="IAsyncLockQueueStrategy"/>.
    /// </summary>
    public class LifoLockQueueStrategy : IAsyncLockQueueStrategy
    {
        private readonly Stack<TaskCompletionSource<IAsyncDisposable>> _stack = new();

        /// <inheritdoc />
        public void Enqueue(TaskCompletionSource<IAsyncDisposable> tcs)
        {
            lock (_stack)
            {
                _stack.Push(tcs);
            }
        }

        /// <inheritdoc />
        public bool TryDequeue(out TaskCompletionSource<IAsyncDisposable>? tcs)
        {
            lock (_stack)
            {
                if (_stack.Count > 0)
                {
                    tcs = _stack.Pop();
                    return true;
                }

                tcs = null;
                return false;
            }
        }
    }
}
