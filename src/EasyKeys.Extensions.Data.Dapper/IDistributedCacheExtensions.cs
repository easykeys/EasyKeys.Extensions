using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Caching.Distributed
{
    public static class IDistributedCacheExtensions
    {
        public static async Task SetAsync<T>(
            this IDistributedCache cache,
            string key,
            T value,
            DistributedCacheEntryOptions options,
            CancellationToken cancellationToken = default)
           where T : class, new()
        {
            using var stream = new MemoryStream();
            new BinaryFormatter().Serialize(stream, value);
            await cache.SetAsync(key, stream.ToArray(), options, cancellationToken);
        }

        public static async Task SetAsync<T>(
            this IDistributedCache cache,
            string key,
            IEnumerable<T> value,
            DistributedCacheEntryOptions options,
            CancellationToken cancellationToken = default)
                where T : class, new()
        {
            using var stream = new MemoryStream();
            new BinaryFormatter().Serialize(stream, value);
            await cache.SetAsync(key, stream.ToArray(), options, cancellationToken);
        }

        public static async Task<T?> GetAsync<T>(
            this IDistributedCache cache,
            string key,
            CancellationToken cancellationToken = default)
             where T : class, new()
        {
            var data = await cache.GetAsync(key, cancellationToken);
            if (data == null)
            {
                return default;
            }

            using var stream = new MemoryStream(data);
            {
                return (T)new BinaryFormatter().Deserialize(stream);
            }
        }

        public static async Task<IEnumerable<T>?> GetManyAsync<T>(
            this IDistributedCache cache,
            string key,
            CancellationToken cancellationToken = default)
                        where T : class, new()
        {
            var data = await cache.GetAsync(key, cancellationToken);

            if (data == null)
            {
                return null;
            }

            using var stream = new MemoryStream(data);
            {
                return (IEnumerable<T>)new BinaryFormatter().Deserialize(stream);
            }
        }

        public static async Task SetAsync(
            this IDistributedCache cache,
            string key,
            string value,
            DistributedCacheEntryOptions options,
            CancellationToken cancellationToken = default)
        {
            await cache.SetAsync(key, Encoding.UTF8.GetBytes(value), options, cancellationToken);
        }

        public static async Task<string?> GetAsync(
            this IDistributedCache cache,
            string key,
            CancellationToken cancellationToken = default)
        {
            var data = await cache.GetAsync(key, cancellationToken);
            if (data != null)
            {
                return Encoding.UTF8.GetString(data);
            }

            return null;
        }

        /// <summary>
        /// Gets an instance from the cache, or uses the provided factory to load the instance, store it in the cache and return the value.
        /// </summary>
        /// <typeparam name="T">Type of object to pull from the cache.</typeparam>
        /// <param name="cache">The instance of the cache object to pull from.</param>
        /// <param name="key">The key to pull.</param>
        /// <param name="factory">The factory function used to load the value if it isn't already in the cache.</param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>An instance of the type of object.</returns>
        public static async Task<T> GetOrCreateAsync<T>(
            this IDistributedCache cache,
            string key,
            Func<T> factory,
            DistributedCacheEntryOptions options,
            CancellationToken cancellationToken = default)
                where T : class, new()
        {
            var cached = await cache.GetAsync<T>(key, cancellationToken);
            if (cached == null)
            {
                cached = factory();
                await cache.SetAsync(key, cached, options, cancellationToken);
            }

            return cached;
        }

        /// <summary>
        /// Gets an instance from the cache, or uses the provided factory to load the instance, store it in the cache and return the value.
        /// </summary>
        /// <typeparam name="T">Type of object to pull from the cache.</typeparam>
        /// <param name="cache">The instance of the cache object to pull from.</param>
        /// <param name="key">The key to pull.</param>
        /// <param name="factory">The factory function used to load the value if it isn't already in the cache.</param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>An instance of the type of object.</returns>
        public static async Task<T> GetOrCreateAsync<T>(
            this IDistributedCache cache,
            string key,
            Func<Task<T>> factory,
            DistributedCacheEntryOptions options,
            CancellationToken cancellationToken = default)
            where T : class, new()
        {
            var cached = await cache.GetAsync<T>(key, cancellationToken);
            if (cached == null)
            {
                cached = await factory();
                await cache.SetAsync<T>(key, cached, options, cancellationToken);
            }

            return cached;
        }
    }
}
