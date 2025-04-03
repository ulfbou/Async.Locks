// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Async.Locks;

using BenchmarkDotNet.Attributes;

using System.Diagnostics;

namespace Async.Lock.Benchmarks
{
    [MemoryDiagnoser]
    public class AsyncLockBenchmarks
    {
        private AsyncLock _asyncLock = default!;

        [Params(100, 1000, 10000)]
        public int Operations;

        private int _acquireCount;
        private int _releaseCount;
        private int _timeoutCount;
        private int _cancelCount;

        [GlobalSetup]
        public void Setup()
        {
            _asyncLock = new AsyncLock();
            _acquireCount = 0;
            _releaseCount = 0;
            _timeoutCount = 0;
            _cancelCount = 0;

            _asyncLock.OnLockAcquired += () => Interlocked.Increment(ref _acquireCount);
            _asyncLock.OnLockReleased += () => Interlocked.Increment(ref _releaseCount);
            _asyncLock.OnLockTimeout += () => Interlocked.Increment(ref _timeoutCount);
            _asyncLock.OnLockCancelled += () => Interlocked.Increment(ref _cancelCount);
        }

        [Benchmark]
        public async Task ConcurrentAcquireReleaseBenchmark()
        {
            await Parallel.ForEachAsync(Enumerable.Range(0, Operations), async (i, _) =>
            {
                await using var handle = await _asyncLock.AcquireAsync(TimeSpan.FromMilliseconds(100), default);
                await Task.Delay(10);
            });

            // Correctness Validation: Ensure all acquisitions and releases occurred.
            Debug.Assert(_acquireCount == Operations, "Acquire count mismatch.");
            Debug.Assert(_releaseCount == Operations, "Release count mismatch.");
        }

        [Benchmark]
        public async Task CancellationBenchmark()
        {
            var cts = new CancellationTokenSource();
            var tasks = Enumerable.Range(0, Operations)
                .Select(async i =>
                {
                    try
                    {
                        await using var handle = await _asyncLock.AcquireAsync(TimeSpan.FromMilliseconds(50), cts.Token);
                        await Task.Delay(1000, cts.Token);
                    }
                    catch (OperationCanceledException) { }
                });

            cts.Cancel();
            await Task.WhenAll(tasks);

            // Correctness Validation: Ensure all tasks were cancelled.
            Debug.Assert(_cancelCount == Operations, "Cancel count mismatch.");
        }

        [Benchmark]
        public async Task PartialCancellationBenchmark()
        {
            var cts = new CancellationTokenSource();
            var tasks = Enumerable.Range(0, Operations)
                .Select(async i =>
                {
                    try
                    {
                        if (i % 2 == 0)
                        {
                            await using var handle = await _asyncLock.AcquireAsync(TimeSpan.FromMilliseconds(50), cts.Token);
                            await Task.Delay(1000, cts.Token);
                        }
                        else
                        {
                            await using var handle = await _asyncLock.AcquireAsync(TimeSpan.FromMilliseconds(50), default);
                            await Task.Delay(10);
                        }
                    }
                    catch (OperationCanceledException) { }
                });

            cts.Cancel();
            await Task.WhenAll(tasks);

            // Correctness Validation: Ensure half the tasks were cancelled.
            Debug.Assert(_cancelCount == Operations / 2, "Partial cancel count mismatch.");
        }

        [Benchmark]
        public async Task ConcurrentCancellationBenchmark()
        {
            var cts = new CancellationTokenSource();
            var tasks = Enumerable.Range(0, Operations)
                .Select(async i =>
                {
                    try
                    {
                        await using var handle = await _asyncLock.AcquireAsync(TimeSpan.FromMilliseconds(100), cts.Token);
                        await Task.Delay(1000, cts.Token);
                    }
                    catch (OperationCanceledException) { }
                })
                .ToList();

            var cancellationTasks = Enumerable.Range(0, Operations / 2)
                .Select(async i =>
                {
                    await Task.Delay(50);
                    cts.Cancel();
                });

            await Task.WhenAll(tasks.Concat(cancellationTasks));

            // Correctness Validation: Ensure half the tasks were cancelled concurrently.
            Debug.Assert(_cancelCount == Operations / 2, "Concurrent cancel count mismatch.");
        }

        [Benchmark]
        public async Task MixedLockAcquisitionBenchmark()
        {
            var cts = new CancellationTokenSource();
            var random = new Random();

            var tasks = Enumerable.Range(0, Operations)
                .Select(async i =>
                {
                    try
                    {
                        var timeout = random.Next(50, 200);
                        await using var handle = await _asyncLock.AcquireAsync(TimeSpan.FromMilliseconds(timeout), cts.Token);
                        await Task.Delay(random.Next(10, 100), cts.Token);
                    }
                    catch (OperationCanceledException) { }
                })
                .ToList();

            cts.CancelAfter(Operations / 2 * 75);

            await Task.WhenAll(tasks);

            // Correctness Validation: Ensure correct counts for mixed scenarios.
            // (Adjust expected counts based on timeout and cancellation logic)

            Debug.Assert(_cancelCount + _timeoutCount <= Operations, "Timeout or cancel count is too high");
            Debug.Assert(_acquireCount <= Operations, "Acquire count is too high");
            Debug.Assert(_releaseCount <= Operations, "release count is too high");
        }
    }
}
