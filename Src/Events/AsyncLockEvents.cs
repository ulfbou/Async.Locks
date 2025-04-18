// Copyright (c) Ulf Bourelius. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.Tracing;

namespace Async.Locks.Events
{
    /// <summary>
    /// Represents an event source for logging async lock events.
    /// </summary>
    /// remarks>
    /// This class is used to log events related to async locks, such as task start, completion, queue operations, and lock acquisition/release.
    /// It uses the <see cref="EventSource"/> class to provide a lightweight logging mechanism.
    /// </remarks>
    [EventSource(Name = "AsyncLocks")]
    public class AsyncLockEvents : EventSource
    {
        /// <summary>
        /// Singleton instance of the <see cref="AsyncLockEvents"/> class.
        /// </summary>
        public static AsyncLockEvents Log = new AsyncLockEvents();

        /// <summary>
        /// Logs when a task starts.
        /// </summary>
        /// <param name="taskId">The ID of the task that started.</param>
        /// <remarks>
        /// This event is logged when a task starts executing.
        /// </remarks>
        [Event(1, Level = EventLevel.Informational)]
        public void TaskStarted(int taskId) { WriteEvent(1, taskId); }

        /// <summary>
        /// Logs when a task completes.
        /// </summary>
        /// <param name="taskId">The ID of the task that completed.</param>
        /// <remarks>
        /// This event is logged when a task completes its execution.
        /// </remarks>
        [Event(2, Level = EventLevel.Informational)]
        public void TaskCompleted(int taskId) { WriteEvent(2, taskId); }

        /// <summary>
        /// Logs when a task completes.
        /// </summary>
        /// <param name="taskId">The ID of the task that completed.</param>
        /// <remarks>
        /// This event is logged when a task completes its execution.
        /// </remarks>
        [Event(3, Level = EventLevel.Informational)]
        public void QueueEnqueue(int taskId) { WriteEvent(3, taskId); }

        /// <summary>
        /// Logs when a task is dequeued from the queue.
        /// </summary>
        /// <param name="taskId">The ID of the task that completed.</param>
        /// <remarks>
        /// This event is logged when a task completes its execution.
        /// </remarks>
        [Event(4, Level = EventLevel.Informational)]
        public void QueueDequeue(int taskId) { WriteEvent(4, taskId); }

        /// <summary>
        /// Logs when a task is acquired.
        /// </summary>
        /// <param name="taskId">The ID of the task that completed.</param>
        /// <remarks>
        /// This event is logged when a task completes its execution.
        /// </remarks>
        [Event(5, Level = EventLevel.Informational)]
        public void LockAcquired(int taskId) { WriteEvent(5, taskId); }

        /// <summary>
        /// Logs when a task is released.
        /// </summary>
        /// <param name="taskId">The ID of the task that completed.</param>
        /// <remarks>
        /// This event is logged when a task completes its execution.
        /// </remarks>
        [Event(6, Level = EventLevel.Informational)]
        public void LockReleased(int taskId) { WriteEvent(6, taskId); }

        /// <summary>
        /// Logs when a task times out.
        /// </summary>
        /// <param name="taskId">The ID of the task that completed.</param>
        /// <remarks>
        /// This event is logged when a task completes its execution.
        /// </remarks>
        [Event(7, Level = EventLevel.Informational)]
        public void LockTimeout(int taskId) { WriteEvent(7, taskId); }

        /// <summary>
        /// Logs when a task is cancelled.
        /// </summary>
        /// <param name="taskId">The ID of the task that completed.</param>
        /// <remarks>
        /// This event is logged when a task completes its execution.
        /// </remarks>
        [Event(8, Level = EventLevel.Informational)]
        public void LockCancelled(int taskId) { WriteEvent(8, taskId); }
    }
}
