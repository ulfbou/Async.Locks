// Copyright (c) Ulf Bourelius. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.Tracing;

namespace Async.Locks.Monitoring
{
    /// <summary>
    /// A class that monitors async lock events using <see cref="EventSource"/>.
    /// </summary>
    public class AsyncLockMonitor : EventListener
    {
        private readonly List<(string EventName, int TaskId, DateTime Timestamp)> _events = new();
        private bool _enabled = false;

        /// <inheritdoc />
        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (eventSource.Name == "AsyncLocks" && _enabled)
            {
                EnableEvents(eventSource, EventLevel.Informational);
            }
        }

        /// <inheritdoc />
        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            _events.Add((eventData.EventName ?? string.Empty, eventData.Payload?[0] as int? ?? -1, DateTime.UtcNow));
        }

        /// <summary>
        /// Gets the recorded events.
        /// </summary>
        public IEnumerable<(string EventName, int TaskId, DateTime Timestamp)> GetEvents() => _events;

        /// <summary>
        /// Resets the recorded events.
        /// </summary>
        public void Reset() => _events.Clear();

        /// <summary>
        /// Enables the monitoring of async lock events.
        /// </summary>
        public void Enable()
        {
            _enabled = true;

            foreach (var source in EventSource.GetSources())
            {
                OnEventSourceCreated(source);
            }
        }
    }
}
