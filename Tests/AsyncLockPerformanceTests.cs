// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;

using System.Diagnostics;

using Xunit;

namespace Async.Locks.Tests
{
    public class AsyncLockPerformanceTests
    {
        private const int TestTimeoutMilliseconds = 5000;

        [Fact]
        public async Task HighContention_Performance()
        {
            IAsyncLock asyncLock = new TestAsyncLock();
            int concurrentTasks = 1000;
            var stopwatch = Stopwatch.StartNew();

            var tasks = Enumerable.Range(0, concurrentTasks).Select(async _ =>
            {
                await using var releaser = await asyncLock.AcquireAsync();
            }).ToList();

            await Task.WhenAll(tasks).TimeoutAfter(TimeSpan.FromMilliseconds(TestTimeoutMilliseconds));
            stopwatch.Stop();

            (stopwatch.ElapsedMilliseconds < 5000).Should().BeTrue();
        }
    }
}
