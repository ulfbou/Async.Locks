// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

using Async.Locks;

class Program
{
    private static readonly AsyncLock _asyncLock = new AsyncLock();

    static async Task Main(string[] args)
    {
        // Simulating multiple tasks accessing a resource
        var task1 = PrintMessageAsync("Hello from Task 1");
        var task2 = PrintMessageAsync("Hello from Task 2");
        var task3 = PrintMessageAsync("Hello from Task 3");

        await Task.WhenAll(task1, task2, task3);
    }

    private static async Task PrintMessageAsync(string message)
    {
        // Use the AsyncLock to acquire a lock before accessing the resource
        await using (await _asyncLock.AcquireAsync())
        {
            Console.WriteLine(message);
            await Task.Delay(1000); // Simulate some work
        }
    }
}
