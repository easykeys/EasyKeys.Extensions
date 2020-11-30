using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;

namespace EasyKeys.Extensions.Caching
{
    public class MemoryDistributedCacheExtended : IDistributedCacheExtended
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryDistributedCacheExtended(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public Task ClearAsync()
        {
            return Task.Run(() => (_memoryCache as MemoryCache)?.Compact(100));
        }

        public Task<IEnumerable<string>> GetKeysAsync()
        {
            return Task.Run(() =>
            {
                var result = new List<string>();

                return result as IEnumerable<string>;
            });
        }

        public Task RemoveAsync(string[] keys)
        {
            return Task.Run(() =>
            {
                foreach (var key in keys)
                {
                    _memoryCache.Remove(key);
                }
            });
        }
    }
}
