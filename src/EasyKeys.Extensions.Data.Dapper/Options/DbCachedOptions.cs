using Microsoft.Extensions.Caching.Distributed;

namespace EasyKeys.Extensions.Data.Dapper.Options
{
    public class DbCachedOptions : DbOptions
    {
        public DistributedCacheEntryOptions CacheOptions { get; set; } = new DistributedCacheEntryOptions();
    }
}
