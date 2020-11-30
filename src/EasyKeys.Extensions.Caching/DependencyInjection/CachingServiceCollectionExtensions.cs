using System;

using EasyKeys.Extensions.Caching;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CachingServiceCollectionExtensions
    {
        public static IServiceCollection AddRedisDistributedCaching(
            this IServiceCollection services,
            Action<RedisCacheOptions>? setupAction = default)
        {
            services.AddStackExchangeRedisCache(setupAction);

            services.TryAddSingleton<IDistributedCacheExtended, StackExchangeRedisDistributedCacheExtended>();

            return services;
        }

        public static IServiceCollection AddDistributedMemoryCaching(
            this IServiceCollection services,
            Action<MemoryDistributedCacheOptions>? setupAction = default)
        {
            if (setupAction == null)
            {
                services.AddDistributedMemoryCache();
            }
            else
            {
                services.AddDistributedMemoryCache(setupAction);
            }

            services.TryAddSingleton<IDistributedCacheExtended, MemoryDistributedCacheExtended>();

            return services;
        }
    }
}
