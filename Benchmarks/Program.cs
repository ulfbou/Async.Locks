// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using BenchmarkDotNet.Running;

namespace Async.Lock.Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<AsyncLockQueueStrategyBenchmarks>();
            BenchmarkRunner.Run<AsyncLockBenchmarks>();
        }
    }
}
