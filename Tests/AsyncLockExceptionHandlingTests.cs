// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;

using Xunit;

namespace Async.Locks.Tests
{
    public class AsyncLockExceptionHandlingTests
    {
        [Fact]
        public async Task Exception_WithinCriticalSection_ReleasesLock()
        {
            IAsyncLock asyncLock = new TestAsyncLock();

            try
            {
                await using var releaser = await asyncLock.AcquireAsync();
                throw new InvalidOperationException("Test Exception");
            }
            catch (InvalidOperationException) { }

            await using var releaser2 = await asyncLock.AcquireAsync();
            releaser2.Should().NotBeNull();
        }
    }
}
