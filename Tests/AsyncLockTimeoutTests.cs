using FluentAssertions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace Async.Locks.Tests
{
    public class AsyncLockTimeoutTests
    {
        [Fact]
        public async Task LockAsync_Timeout_Expires_ThrowsTimeoutException()
        {
            // Arrange
            IAsyncLock asyncLock = new AsyncLock();
            var timeout = TimeSpan.FromMilliseconds(100);

            // Acquire the lock to ensure the next attempt will timeout
            var releaser = await asyncLock.LockAsync();

            // Act
            var exception = await Record.ExceptionAsync(async () => await asyncLock.LockAsync(timeout: timeout));

            // Assert
            exception.Should().BeOfType<TimeoutException>();
        }

        [Fact]
        public async Task LockAsync_Timeout_DoesNotExpire_Success()
        {
            // Arrange
            IAsyncLock asyncLock = new AsyncLock();
            var timeout = TimeSpan.FromMilliseconds(100);

            // Acquire and release the lock
            await using (var releaser = await asyncLock.LockAsync())
            {
                releaser.Should().NotBeNull();
            }

            // Act
            var exception = await Record.ExceptionAsync(async () => await asyncLock.LockAsync(timeout: timeout));

            // Assert
            exception.Should().BeNull();
        }
    }
}
