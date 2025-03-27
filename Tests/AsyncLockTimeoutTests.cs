// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;

using Xunit;

namespace Async.Locks.Tests
{
    public class AsyncLockTimeoutTests
    {
        private const int TestTimeoutMilliseconds = 100;
        private const int MaxTimeoutMilliseconds = 10000;

        [Fact]
        public async Task LockAsync_Timeout_Expires_ThrowsTimeoutException()
        {
            IAsyncLock asyncLock = new AsyncLock();
            var timeout = TimeSpan.FromMilliseconds(100);
            var releaser = await asyncLock.AcquireAsync();

            var exception = await Record.ExceptionAsync(async () => await asyncLock.AcquireAsync(timeout: timeout).TimeoutAfter(TimeSpan.FromMilliseconds(MaxTimeoutMilliseconds)));

            exception.Should().BeOfType<TimeoutException>();
        }

        [Fact]
        public async Task LockAsync_Timeout_DoesNotExpire_Success()
        {
            IAsyncLock asyncLock = new AsyncLock();
            var timeout = TimeSpan.FromMilliseconds(100);


            await using (await asyncLock.AcquireAsync())
            {
                await Task.Delay(50);
            }

            var exception = await Record.ExceptionAsync(async () => await asyncLock.AcquireAsync(timeout: timeout));

            exception.Should().BeNull();
        }

        [Fact]
        public async Task LockAsync_ZeroLengthTimeout_ThrowsTimeoutException()
        {
            IAsyncLock asyncLock = new AsyncLock();
            var timeout = TimeSpan.Zero;
            var releaser = await asyncLock.AcquireAsync();

            var exception = await Record.ExceptionAsync(async () => await asyncLock.AcquireAsync(timeout: timeout).TimeoutAfter(TimeSpan.FromMilliseconds(MaxTimeoutMilliseconds)));

            exception.Should().BeOfType<TimeoutException>();
        }

        [Fact]
        public async Task LockAsync_VeryShortTimeout_ThrowsTimeoutException()
        {
            IAsyncLock asyncLock = new AsyncLock();
            var timeout = TimeSpan.FromMilliseconds(1);
            var releaser = await asyncLock.AcquireAsync();

            var exception = await Record.ExceptionAsync(async () => await asyncLock.AcquireAsync(timeout: timeout).TimeoutAfter(TimeSpan.FromMilliseconds(MaxTimeoutMilliseconds)));

            exception.Should().BeOfType<TimeoutException>();
        }

        [Fact]
        public async Task LockAsync_VeryLongTimeout_Success()
        {
            IAsyncLock asyncLock = new AsyncLock();
            var timeout = TimeSpan.FromMinutes(1);

            await using (await asyncLock.AcquireAsync())
            {

            }

            var exception = await Record.ExceptionAsync(async () => await asyncLock.AcquireAsync(timeout: timeout));

            exception.Should().BeNull();
        }

        [Fact]
        public async Task LockAsync_NegativeTimeout_ThrowsArgumentOutOfRangeException()
        {
            IAsyncLock asyncLock = new AsyncLock();
            var timeout = TimeSpan.FromMilliseconds(-1);

            var exception = await Record.ExceptionAsync(async () => await asyncLock.AcquireAsync(timeout: timeout).TimeoutAfter(TimeSpan.FromMilliseconds(MaxTimeoutMilliseconds)));

            exception.Should().BeOfType<ArgumentOutOfRangeException>();
        }
    }
}
