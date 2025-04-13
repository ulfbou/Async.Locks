// Copyright (c) Ulf Bourelius. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.Tracing;

namespace Async.Locks.Events
{
    [EventSource(Name = "AsyncLocks")]
    public class AsyncLockEvents : EventSource
    {
        public static AsyncLockEvents Log = new AsyncLockEvents();

        [Event(1, Level = EventLevel.Informational)]
        public void TaskStarted(int taskId) { WriteEvent(1, taskId); }

        [Event(2, Level = EventLevel.Informational)]
        public void TaskCompleted(int taskId) { WriteEvent(2, taskId); }

        [Event(3, Level = EventLevel.Informational)]
        public void QueueEnqueue(int taskId) { WriteEvent(3, taskId); }

        [Event(4, Level = EventLevel.Informational)]
        public void QueueDequeue(int taskId) { WriteEvent(4, taskId); }

        [Event(5, Level = EventLevel.Informational)]
        public void LockAcquired(int taskId) { WriteEvent(5, taskId); }

        [Event(6, Level = EventLevel.Informational)]
        public void LockReleased(int taskId) { WriteEvent(6, taskId); }

        [Event(7, Level = EventLevel.Informational)]
        public void LockTimeout(int taskId) { WriteEvent(7, taskId); }

        [Event(8, Level = EventLevel.Informational)]
        public void LockCancelled(int taskId) { WriteEvent(8, taskId); }
    }
}
