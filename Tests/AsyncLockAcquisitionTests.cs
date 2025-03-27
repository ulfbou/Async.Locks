// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;

using Xunit;

namespace Async.Locks.Tests
{
    public class AsyncLockAcquisitionTests
    {
        [Fact]
        public async Task SingleThreaded_AcquireAndRelease_Success()
        {
            IAsyncLock asyncLock = new TestAsyncLock();

            await using var releaser = await asyncLock.AcquireAsync();

            releaser.Should().NotBeNull();
        }

        [Fact]
        public async Task SingleThreaded_AcquireAndRelease_MultipleTimes_Success()
        {
            IAsyncLock asyncLock = new TestAsyncLock();

            for (int i = 0; i < 10; i++)
            {
                await using var releaser = await asyncLock.AcquireAsync();

                releaser.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task MultiThreaded_ConcurrentAcquisition_Success()
        {
            IAsyncLock asyncLock = new TestAsyncLock();
            int concurrentTasks = 100;

            var tasks = Enumerable.Range(0, concurrentTasks).Select(async _ =>
            {
                await using var releaser = await asyncLock.AcquireAsync();

                releaser.Should().NotBeNull();
                await Task.Delay(10);
            }).ToList();

            await Task.WhenAll(tasks);
        }
    }
}
