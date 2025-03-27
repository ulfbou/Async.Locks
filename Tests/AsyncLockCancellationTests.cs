// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;

using System;

using Xunit;

namespace Async.Locks.Tests
{
    public class AsyncLockCancellationTests
    {
        private const int TestTimeoutMilliseconds = 5000;

        [Fact]
        public async Task Cancellation_AfterLockAcquired_DoesNotAffectLock()
        {
            IAsyncLock asyncLock = new AsyncLock();
            var cts = new CancellationTokenSource();

            var releaser = await asyncLock.AcquireAsync(cancellationToken: cts.Token);
            cts.Cancel();

            releaser.Should().NotBeNull();
            await asyncLock.ReleaseAsync();
        }

        [Fact]
        public async Task Cancellation_WhileLockBeingReleased_DoesNotAffectLock()
        {
            IAsyncLock asyncLock = new AsyncLock();
            var cts = new CancellationTokenSource();

            var releaser = await asyncLock.AcquireAsync();
            var releaseTask = asyncLock.ReleaseAsync();
            cts.Cancel();

            await releaseTask;
            cts.Token.IsCancellationRequested.Should().BeTrue();
        }

        [Fact]
        public async Task Cancellation_WithOneWaitingTask_NoCancellation()
        {
            IAsyncLock asyncLock = new AsyncLock();
            var releaser = await asyncLock.AcquireAsync();

            var task = asyncLock.AcquireAsync();
            var task2 = asyncLock.AcquireAsync();
            var task3 = asyncLock.AcquireAsync();

            await releaser.DisposeAsync();
            await asyncLock.ReleaseAsync();
            await asyncLock.ReleaseAsync();
            await asyncLock.ReleaseAsync();

            await Task.WhenAny(task.AsTask(), Task.Delay(10000));
            await Task.WhenAny(task2.AsTask(), Task.Delay(10000));
            await Task.WhenAny(task3.AsTask(), Task.Delay(10000));

            task.IsCompleted.Should().BeTrue();
            task2.IsCompleted.Should().BeTrue();
            task3.IsCompleted.Should().BeTrue();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(1000)]
        public async Task LockAsync_Timeout_Expires_ThrowsTimeoutException(int timeoutMilliseconds)
        {
            IAsyncLock asyncLock = new AsyncLock();
            var timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
            var releaser = await asyncLock.AcquireAsync();

            var exception = await Record.ExceptionAsync(async () => await asyncLock.AcquireAsync(timeout: timeout));

            exception.Should().BeOfType<TimeoutException>();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(-1000)]
        public async Task LockAsync_WithNonPositiveTimeout_ThrowsArgumentOutOfRangeException(int timeoutMilliseconds)
        {
            IAsyncLock asyncLock = new AsyncLock();
            var timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
            var releaser = await asyncLock.AcquireAsync();
            var exception = await Record.ExceptionAsync(async () => await asyncLock.AcquireAsync(timeout: timeout).TimeoutAfter(timeout));
            exception.Should().BeOfType<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData(100)]
        [InlineData(1000)]
        public async Task Cancellation_WithOneWaitingTask_CancellationOnly(int timeoutMilliseconds)
        {
            IAsyncLock asyncLock = new AsyncLock();
            TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
            var tokenSource = new CancellationTokenSource();
            var releaser = await asyncLock.AcquireAsync();

            var task = asyncLock.AcquireAsync(timeout, cancellationToken: tokenSource.Token);
            tokenSource.Cancel();

            try
            {
                await task;
                Assert.Fail("task should have been canceled");
            }
            catch (TaskCanceledException)
            {
                Assert.True(true);
            }

            await releaser.DisposeAsync();
        }
    }
}
