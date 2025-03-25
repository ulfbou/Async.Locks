// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Async.Locks
{
    /// <inheritdoc />
    public class AsyncLock : AsyncLockBase, IAsyncLock
    {
        private readonly object _lock = new object();
        private readonly Queue<TaskCompletionSource<AsyncReleaser<AsyncLock>>> _pendingReleasers = new();
        private bool _isLocked = false;
        private bool _isCanceled = false;
        private readonly ManualResetEventSlim _cancellationEvent = new ManualResetEventSlim(false);

        public event EventHandler? LockAcquired;
        public event EventHandler? LockReleased;

        public override async Task<IAsyncDisposable> LockAsync(CancellationToken cancellationToken = default)
        {
            TaskCompletionSource<AsyncReleaser<AsyncLock>> tcs = new TaskCompletionSource<AsyncReleaser<AsyncLock>>(TaskCreationOptions.RunContinuationsAsynchronously);

            Interlocked.Increment(ref _acquisitionCount);

            lock (_lock)
            {
                if (!_isLocked)
                {
                    _isLocked = true;
                    LockAcquired?.Invoke(this, EventArgs.Empty);
                    return new AsyncReleaser<AsyncLock>(this);
                }
                else
                {
                    _pendingReleasers.Enqueue(tcs);
                    cancellationToken.Register(() =>
                    {
                        lock (_lock)
                        {
                            if (!tcs.Task.IsCompleted)
                            {
                                _isCanceled = true;
                                _cancellationEvent.Set();
                                tcs.TrySetCanceled(cancellationToken);
                                _pendingReleasers.TryDequeue(out TaskCompletionSource<AsyncReleaser<AsyncLock>>? removedTcs);
                                if (removedTcs == tcs)
                                {
                                    // Ensure the task is removed from the queue.
                                }
                            }
                        }
                    });
                }
            }

            try
            {
                await tcs.Task.ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                if (_isCanceled)
                {
                    throw;
                }
            }

            if (_isCanceled)
            {
                _cancellationEvent.Wait(cancellationToken);
            }

            return new AsyncReleaser<AsyncLock>(this);
        }

        void IAsyncLock.Release()
        {
            lock (_lock)
            {
                if (_pendingReleasers.Count > 0)
                {
                    var nextReleaserTcs = _pendingReleasers.Peek();
                    if (!nextReleaserTcs.Task.IsCompleted)
                    {
                        _pendingReleasers.Dequeue();
                        nextReleaserTcs.SetResult(new AsyncReleaser<AsyncLock>(this));
                    }
                    else
                    {
                        _pendingReleasers.Dequeue();
                        Release();
                        return;
                    }
                }
                else
                {
                    _isLocked = false;
                }
            }

            _isCanceled = false;
            _cancellationEvent.Reset();

            Interlocked.Increment(ref _releaseCount);
            LockReleased?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        public override Task<IAsyncDisposable?> TryLockAsync(CancellationToken cancellationToken = default)
        {
            TaskCompletionSource<AsyncReleaser<AsyncLock>> tcs = new TaskCompletionSource<AsyncReleaser<AsyncLock>>(TaskCreationOptions.RunContinuationsAsynchronously);

            Interlocked.Increment(ref _acquisitionCount);

            lock (_lock)
            {
                if (!_isLocked)
                {
                    _isLocked = true;
                    LockAcquired?.Invoke(this, EventArgs.Empty);
                    return Task.FromResult<IAsyncDisposable?>(new AsyncReleaser<AsyncLock>(this));
                }
                else
                {
                    _pendingReleasers.Enqueue(tcs);
                    cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
                }
            }

            return tcs.Task.ContinueWith(t => (IAsyncDisposable?)(t.Result), cancellationToken, TaskContinuationOptions.RunContinuationsAsynchronously, TaskScheduler.Default);
        }

        /// <inheritdoc />
        public override ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        protected override void OnLockAcquired() => LockAcquired?.Invoke(this, EventArgs.Empty);
        protected override void OnLockReleased() => LockReleased?.Invoke(this, EventArgs.Empty);
        internal override void Release() => ((IAsyncLock)this).Release();
    }
}
