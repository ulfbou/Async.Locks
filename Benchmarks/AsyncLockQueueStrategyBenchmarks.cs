// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Async.Locks;

using BenchmarkDotNet.Attributes;

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Async.Lock.Benchmarks
{
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    public class AsyncLockQueueStrategyBenchmarks
    {
        private IAsyncLockQueueStrategy _queueStrategy = default!;
        private TaskCompletionSource<IAsyncDisposable>[] _tasks = default!;

        [Params(100, 1000, 10000)]
        public int Operations;

        public enum CollectionType
        {
            ConcurrentQueue,
            ConcurrentStack,
            ConcurrentBag
        }

        [Params(CollectionType.ConcurrentQueue, CollectionType.ConcurrentStack, CollectionType.ConcurrentBag)]
        public CollectionType SelectedCollectionType;

        [GlobalSetup]
        public void Setup()
        {
            _tasks = new TaskCompletionSource<IAsyncDisposable>[Operations];

            for (int i = 0; i < Operations; i++)
            {
                _tasks[i] = new TaskCompletionSource<IAsyncDisposable>(TaskCreationOptions.RunContinuationsAsynchronously);
            }
        }

        private IAsyncLockQueueStrategy GetQueueStrategy()
        {
            switch (SelectedCollectionType)
            {
                case CollectionType.ConcurrentQueue:
                    return new DefaultQueueStrategy<ConcurrentQueue<TaskCompletionSource<IAsyncDisposable>>>();
                case CollectionType.ConcurrentStack:
                    return new DefaultQueueStrategy<ConcurrentStack<TaskCompletionSource<IAsyncDisposable>>>();
                case CollectionType.ConcurrentBag:
                    return new DefaultQueueStrategy<ConcurrentBag<TaskCompletionSource<IAsyncDisposable>>>();
                default:
                    throw new ArgumentOutOfRangeException(nameof(SelectedCollectionType));
            }
        }

        [Benchmark]
        public void ConcurrentEnqueueBenchmark()
        {
            _queueStrategy = GetQueueStrategy();

            Parallel.For(0, Operations, i =>
            {
                _queueStrategy.Enqueue(_tasks[i]);
            });

            for (int i = 0; i < Operations; i++)
            {
                _queueStrategy.TryDequeue(out _);
            }

            Debug.Assert(_queueStrategy.TryDequeue(out _) == false, "Queue should be empty after dequeueing all tasks.");
        }

        [Benchmark]
        public async Task TimeoutEnforcementBenchmark()
        {
            _queueStrategy = GetQueueStrategy();

            var timeout = TimeSpan.FromMilliseconds(50);
            var tasks = new TaskCompletionSource<IAsyncDisposable>[Operations];
            for (int i = 0; i < Operations; i++)
            {
                tasks[i] = new TaskCompletionSource<IAsyncDisposable>(TaskCreationOptions.RunContinuationsAsynchronously);
                _queueStrategy.Enqueue(tasks[i]);
                await Task.Delay(100); // Exceed timeout
            }

            for (int i = 0; i < Operations; i++)
            {
                _queueStrategy.TryDequeue(out _);
            }

            Debug.Assert(_queueStrategy.TryDequeue(out _) == false, "Queue should be empty after timeout enforcement.");
        }

        [Benchmark]
        public async Task MixedWorkloadBenchmark()
        {
            _queueStrategy = GetQueueStrategy();

            var enqueueCount = Operations * 2 / 3;
            var dequeueCount = Operations / 3;

            var enqueueTasks = Enumerable.Range(0, enqueueCount)
                .Select(i => Task.Run(() => _queueStrategy.Enqueue(_tasks[i])));

            var dequeueTasks = Enumerable.Range(0, dequeueCount)
                .Select(async i =>
                {
                    await Task.Delay(10); // Simulate delay
                    _queueStrategy.TryDequeue(out _);
                });

            await Task.WhenAll(enqueueTasks.Concat(dequeueTasks));

            Debug.Assert(_queueStrategy.TryDequeue(out _) == false, "Queue should be empty after mixed workload.");
        }

        [Benchmark]
        public async Task BurstWorkloadBenchmark()
        {
            _queueStrategy = GetQueueStrategy();

            var random = new Random();
            var tasks = new List<Task>();
            var priorities = new List<int>();

            for (int burstCount = 0; burstCount < 5; burstCount++) // Simulate 5 bursts
            {
                for (int i = 0; i < Operations / 5; i++)
                {
                    var priority = random.Next(1, 10);
                    priorities.Add(priority);
                    tasks.Add(Task.Run(() => _queueStrategy.Enqueue(_tasks[i], priority)));
                }

                await Task.WhenAll(tasks);
                tasks.Clear();

                await Task.Delay(random.Next(50, 200));
            }

            var dequeuedPriorities = new List<int>();
            while (_queueStrategy.TryDequeue(out _))
            {
                dequeuedPriorities.Add(priorities.Max());
                priorities.Remove(priorities.Max());
            }

            Debug.Assert(_queueStrategy.TryDequeue(out _) == false, "Queue should be empty after burst workloads.");
            Debug.Assert(priorities.Count == 0, "All items should be dequeued");
        }

        [Benchmark]
        public async Task ExponentialPriorityBenchmark()
        {
            _queueStrategy = GetQueueStrategy();

            var random = new Random();
            var tasks = new List<Task>();
            var priorities = new List<double>();

            for (int i = 0; i < Operations; i++)
            {
                var priority = Math.Pow(10, -random.NextDouble() * 3);
                priorities.Add(priority);
                tasks.Add(Task.Run(() => _queueStrategy.Enqueue(_tasks[i], (int)priority)));
            }

            await Task.WhenAll(tasks);

            var dequeuedPriorities = new List<double>();
            while (_queueStrategy.TryDequeue(out _))
            {
                dequeuedPriorities.Add(priorities.Max());
                priorities.Remove(priorities.Max());
            }

            Debug.Assert(_queueStrategy.TryDequeue(out _) == false, "Queue should be empty after exponential priority workloads.");
            Debug.Assert(priorities.Count == 0, "All items should be dequeued");
        }

        [Benchmark]
        public Task QueueExhaustionBenchmark()
        {
            _queueStrategy = GetQueueStrategy();

            for (long i = 0; i < 1_000_000; i++)
            {
                _queueStrategy.Enqueue(_tasks[0]);
            }

            return Task.CompletedTask;
        }

        [Benchmark]
        public void EmptyQueueBenchmark()
        {
            _queueStrategy = GetQueueStrategy();

            for (int i = 0; i < 1000; i++)
            {
                _queueStrategy.TryDequeue(out _);
                Debug.Assert(_queueStrategy.TryDequeue(out _) == false, "Empty queue should return false.");
            }
        }

        [Benchmark]
        public Task IdenticalPrioritiesBenchmark()
        {
            _queueStrategy = GetQueueStrategy();

            var dequeuedTasks = new List<TaskCompletionSource<IAsyncDisposable>>();
            for (int i = 0; i < Operations; i++)
            {
                _queueStrategy.Enqueue(_tasks[i], 5);
            }

            for (int i = 0; i < Operations; i++)
            {
                _queueStrategy.TryDequeue(out TaskCompletionSource<IAsyncDisposable>? tcs);
                dequeuedTasks.Add(tcs!);
            }

            for (int i = 0; i < Operations; i++)
            {
                Debug.Assert(dequeuedTasks[i] == _tasks[i], "Tasks should be dequeued in FIFO order");
            }

            return Task.CompletedTask;
        }

        [Benchmark]
        public Task FifoBenchmark()
        {
            _queueStrategy = GetQueueStrategy();

            var dequeuedTasks = new List<TaskCompletionSource<IAsyncDisposable>>();
            var enqueuedTasks = new List<TaskCompletionSource<IAsyncDisposable>>();

            for (int i = 0; i < Operations; i++)
            {
                enqueuedTasks.Add(_tasks[i]);
                _queueStrategy.Enqueue(_tasks[i]);
            }

            for (int i = 0; i < Operations; i++)
            {
                _queueStrategy.TryDequeue(out TaskCompletionSource<IAsyncDisposable>? tcs);
                dequeuedTasks.Add(tcs!);
            }

            for (int i = 0; i < Operations; i++)
            {
                switch (SelectedCollectionType)
                {
                    case CollectionType.ConcurrentQueue:
                        Debug.Assert(dequeuedTasks[i] == enqueuedTasks[i], "Tasks should be dequeued in FIFO order.");
                        break;
                    case CollectionType.ConcurrentStack:
                        Debug.Assert(dequeuedTasks[i] == enqueuedTasks[Operations - i - 1], "Tasks should be dequeued in LIFO order.");
                        break;
                    case CollectionType.ConcurrentBag:
                        // ConcurrentBag does not guarantee order, so we can't make order assertions.
                        break;
                }
            }

            return Task.CompletedTask;
        }

        [Benchmark]
        public Task MixedPriorityFifoBenchmark()
        {
            _queueStrategy = GetQueueStrategy();

            var random = new Random();
            var dequeuedTasks = new List<TaskCompletionSource<IAsyncDisposable>>();
            var enqueuedTasks = new List<TaskCompletionSource<IAsyncDisposable>>();
            var priorityList = new List<int>();

            for (int i = 0; i < Operations; i++)
            {
                var priority = random.Next(0, 2) == 0 ? random.Next(1, 10) : 0; // Mix priorities and no priority
                priorityList.Add(priority);
                enqueuedTasks.Add(_tasks[i]);
                _queueStrategy.Enqueue(_tasks[i], priority);
            }

            for (int i = 0; i < Operations; i++)
            {
                _queueStrategy.TryDequeue(out TaskCompletionSource<IAsyncDisposable>? tcs);
                dequeuedTasks.Add(tcs!);
            }

            // add validation logic to ensure that priority is respected, and that when priority is the same, or 0, that FIFO or LIFO order is respected.
            return Task.CompletedTask;
        }
    }
}
