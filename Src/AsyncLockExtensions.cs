// Copyright (c) Async Framework projects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Async.Locks
{
    public static class AsyncLockExtensions
    {
        public static IServiceCollection AddAsyncLock(this IServiceCollection services, TimeSpan? defaultTimeout = null, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            services.Add(new ServiceDescriptor(typeof(IAsyncLock), provider => new AsyncLock(defaultTimeout), lifetime));
            return services;
        }
    }
}
