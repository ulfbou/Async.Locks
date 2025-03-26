// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Async.Locks
{
    public class AsyncLock : AsyncLockBase, IAsyncLock
    {
        public AsyncLock(IAsyncLockQueueStrategy? queueStrategy = null) : base(queueStrategy) { }
    }
}
