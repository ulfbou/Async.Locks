// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Async.Locks
{
    /// <summary>
    /// Provides event arguments for reader-writer lock events.
    /// </summary>
    public class ReaderWriterLockEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReaderWriterLockEventArgs"/> class.
        /// </summary>
        /// <param name="threadId">The ID of the thread that acquired or released the lock.</param>
        public ReaderWriterLockEventArgs(int threadId)
        {
            ThreadId = threadId;
        }

        /// <summary>
        /// Gets the ID of the thread that acquired or released the lock.
        /// </summary>
        public int ThreadId { get; }
    }
}
