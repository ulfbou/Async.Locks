// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Async.Locks.Tests
{
    public static class ValueTaskExtensions
    {
        public static async ValueTask TimeoutAfter(this ValueTask task, TimeSpan timeout)
        {
            if (await Task.WhenAny(task.AsTask(), Task.Delay(timeout)) == task.AsTask())
            {
                await task;
            }
            else
            {
                throw new TimeoutException("The operation has timed out.");
            }
        }

        public static async ValueTask<T> TimeoutAfter<T>(this ValueTask<T> task, TimeSpan timeout)
        {
            if (await Task.WhenAny(task.AsTask(), Task.Delay(timeout)) == task.AsTask())
            {
                return await task;
            }
            else
            {
                throw new TimeoutException("The operation has timed out.");
            }
        }
    }
}
