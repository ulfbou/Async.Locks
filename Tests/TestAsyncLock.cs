// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;

namespace Async.Locks.Tests
{
    public class TestAsyncLock : AsyncLockBase, IAsyncLock
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly IAsyncLockQueueStrategy _queueStrategy;

        public TestAsyncLock(IAsyncLockQueueStrategy? queueStrategy = null)
        {
            _semaphore = new SemaphoreSlim(1, 1);
            _queueStrategy = queueStrategy ?? new FifoLockQueueStrategy();
        }

        protected override async ValueTask<IAsyncDisposable> AcquireInternalAsync(TimeSpan? timeout, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<IAsyncDisposable>();
            _queueStrategy.Enqueue(tcs);

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (timeout.HasValue)
                {
                    if (timeout.Value.TotalMilliseconds < 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be greater than or equal to zero.");
                    }

                    if (await _semaphore.WaitAsync(timeout.Value, cancellationToken).ConfigureAwait(false))
                    {
                        _queueStrategy.TryDequeue(out _);
                        return new AsyncReleaser<TestAsyncLock>(this);
                    }

                    _queueStrategy.TryDequeue(out var dequeuedTcs);

                    if (dequeuedTcs != null && dequeuedTcs == tcs)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        throw new TimeoutException("TestAsyncLock acquire timed out.");
                    }

                    return await tcs.Task;
                }
                else
                {
                    await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                    _queueStrategy.TryDequeue(out _);
                    return new AsyncReleaser<TestAsyncLock>(this);
                }
            }
            catch (OperationCanceledException)
            {
                _queueStrategy.TryDequeue(out var dequeuedTcs);

                if (dequeuedTcs != null && dequeuedTcs == tcs)
                {
                    throw new TaskCanceledException();
                }

                return await tcs.Task;
            }
        }

        protected override ValueTask ReleaseInternalAsync()
        {
            _semaphore.Release();
            TaskCompletionSource<IAsyncDisposable>? nextTcs;

            if (_queueStrategy.TryDequeue(out nextTcs))
            {
                nextTcs!.SetResult(new AsyncReleaser<TestAsyncLock>(this));
            }

            return ValueTask.CompletedTask;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _semaphore?.Dispose();
            }
        }
    }
}
