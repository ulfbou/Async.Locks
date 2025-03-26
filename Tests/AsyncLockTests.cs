// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Async.Locks;

using FluentAssertions;

using System.Diagnostics;

using Xunit;

namespace Async.Locks.Tests
{
    public class AsyncLockTests
    {
        [Fact]
        public async Task SingleThreaded_AcquireAndRelease_Success()
        {
            IAsyncLock asyncLock = new AsyncLock();
            await using (var releaser = await asyncLock.LockAsync())
            {
                releaser.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task SingleThreaded_AcquireAndRelease_MultipleTimes_Success()
        {
            IAsyncLock asyncLock = new AsyncLock();
            for (int i = 0; i < 10; i++)
            {
                await using (var releaser = await asyncLock.LockAsync())
                {
                    releaser.Should().NotBeNull();
                }
            }
        }

        [Fact]
        public async Task MultiThreaded_ConcurrentAcquisition_Success()
        {
            IAsyncLock asyncLock = new AsyncLock();
            int concurrentTasks = 100;
            var tasks = Enumerable.Range(0, concurrentTasks).Select(async _ =>
            {
                await using (var releaser = await asyncLock.LockAsync())
                {
                    releaser.Should().NotBeNull();
                    await Task.Delay(10); // Simulate work
                }
            }).ToList();

            await Task.WhenAll(tasks);
        }

        [Fact]
        public async Task MultiThreaded_Cancellation_Handled()
        {
            // Arrange
            IAsyncLock asyncLock = new AsyncLock();
            var cts = new CancellationTokenSource();

            var task1 = asyncLock.LockAsync(cancellationToken: cts.Token);

            var task2 = asyncLock.LockAsync(cancellationToken: cts.Token);

            // Act
            cts.Cancel();

            var exception1 = await Record.ExceptionAsync(async () => await task1);
            var exception2 = await Record.ExceptionAsync(async () => await task2);

            // Assert
            exception1.Should().BeNull();
            exception2.Should().BeOfType<TaskCanceledException>();
        }

        [Fact]
        public async Task Exception_WithinCriticalSection_ReleasesLock()
        {
            IAsyncLock asyncLock = new AsyncLock();
            try
            {
                await using (var releaser = await asyncLock.LockAsync())
                {
                    releaser.Should().NotBeNull();
                    throw new InvalidOperationException("Test Exception");
                }
            }
            catch (InvalidOperationException) { }

            await using (var releaser = await asyncLock.LockAsync())
            {
                releaser.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task HighContention_Performance()
        {
            IAsyncLock asyncLock = new AsyncLock();
            int concurrentTasks = 1000;
            var stopwatch = Stopwatch.StartNew();

            var tasks = Enumerable.Range(0, concurrentTasks).Select(async _ =>
            {
                await using (var releaser = await asyncLock.LockAsync())
                {
                    releaser.Should().NotBeNull();
                }
            }).ToList();

            await Task.WhenAll(tasks);
            stopwatch.Stop();
            (stopwatch.ElapsedMilliseconds < 5000).Should().BeTrue();
        }
    }
}
