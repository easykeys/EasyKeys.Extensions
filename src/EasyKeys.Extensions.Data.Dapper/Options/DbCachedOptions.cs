using Microsoft.Extensions.Caching.Distributed;

namespace EasyKeys.Extensions.Data.Dapper.Options;

public class DbCachedOptions
{
    public DistributedCacheEntryOptions CacheOptions { get; set; } = new DistributedCacheEntryOptions();
}
