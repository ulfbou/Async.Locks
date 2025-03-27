# Async.Locks

[![NuGet](https://img.shields.io/nuget/v/Async.Locks.svg)](https://www.nuget.org/packages/Async.Locks)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

`Async.Locks` is a .NET library designed to provide robust and flexible asynchronous locking primitives. It offers a set of tools for managing concurrency in asynchronous applications, ensuring thread safety and preventing race conditions.

## Features

* **Asynchronous Locking:** Provides asynchronous versions of common locking mechanisms.
* **Customizable Queuing:** Supports customizable lock acquisition queuing strategies through the `IAsyncLockQueueStrategy` interface.
* **Timeout and Cancellation:** Allows for timeouts and cancellation of lock acquisition attempts.
* **Event-Driven:** Provides events for lock acquisition, release, timeouts, and cancellations.
* **Extensible Design:** Designed for easy extension and customization.
* **Robust and Thread-Safe:** Ensures thread safety and handles concurrent access correctly.
* **Clear and Consistent API:** Adheres to established .NET naming conventions and design patterns.

## Getting Started

### Installation

Install the `Async.Locks` NuGet package:

```bash
Install-Package Async.Locks
```

### Usage

#### AsyncLock

```csharp
using Async.Locks;

public async Task ExampleAsync()
{
    IAsyncLock asyncLock = new AsyncLock();

    await using (await asyncLock.AcquireAsync())
    {
        // Critical section
        Console.WriteLine("Lock acquired!");
        await Task.Delay(1000); // Simulate work
    }

    Console.WriteLine("Lock released!");
}
```

#### AsyncLock with Timeout

```csharp
using Async.Locks;

public async Task ExampleWithTimeoutAsync()
{
    IAsyncLock asyncLock = new AsyncLock();
    var timeout = TimeSpan.FromMilliseconds(500);

    try
    {
        await using (await asyncLock.AcquireAsync(timeout: timeout))
        {
            // Critical section
            Console.WriteLine("Lock acquired!");
            await Task.Delay(200); // Simulate work
        }
        Console.WriteLine("Lock released!");
    }
    catch (TimeoutException)
    {
        Console.WriteLine("Lock acquisition timed out!");
    }
}
```

#### AsyncLock with Cancellation

```csharp
using Async.Locks;
using System.Threading;

public async Task ExampleWithCancellationAsync()
{
    IAsyncLock asyncLock = new AsyncLock();
    var cancellationTokenSource = new CancellationTokenSource();
    var cancellationToken = cancellationTokenSource.Token;

    cancellationTokenSource.CancelAfter(200);

    try
    {
        await using (await asyncLock.AcquireAsync(cancellationToken: cancellationToken))
        {
            // Critical section
            Console.WriteLine("Lock acquired!");
            await Task.Delay(1000); // Simulate work
        }
        Console.WriteLine("Lock released!");
    }
    catch (TaskCanceledException)
    {
        Console.WriteLine("Lock acquisition cancelled!");
    }
}
```

#### AsyncLock with Custom Queue Strategy

```csharp
using Async.Locks;

public async Task ExampleWithCustomQueueStrategyAsync()
{
    IAsyncLock asyncLock = new AsyncLock(new LifoLockQueueStrategy());

    await using (await asyncLock.AcquireAsync())
    {
        // Critical section
        Console.WriteLine("Lock acquired with LIFO queue!");
        await Task.Delay(1000);
    }
    Console.WriteLine("Lock released!");
}
```

#### Events

```csharp
using Async.Locks;

public async Task ExampleWithEventsAsync()
{
    IAsyncLock asyncLock = new AsyncLock();

    asyncLock.OnLockAcquired += () => Console.WriteLine("Lock acquired event!");
    asyncLock.OnLockReleased += () => Console.WriteLine("Lock released event!");
    asyncLock.OnLockTimeout += () => Console.WriteLine("Lock timeout event!");
    asyncLock.OnLockCancelled += () => Console.WriteLine("Lock cancelled event!");

    await using (await asyncLock.AcquireAsync())
    {
        await Task.Delay(100);
    }
}
```

## Contributing

Contributions are welcome! Please feel free to submit a pull request or open an issue on [GitHub](https://github.com/ulfbou/Async.Locks).

## Documentation

For more detailed documentation and examples, please visit the [GitHub Wiki](https://github.com/ulfbou/Async.Locks/wiki).

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
