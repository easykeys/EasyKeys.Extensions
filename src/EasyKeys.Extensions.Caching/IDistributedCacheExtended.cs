using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Caching.StackExchangeRedis
{
    public interface IDistributedCacheExtended
    {
        Task ClearAsync();

        Task<IEnumerable<string>> GetKeysAsync();

        Task RemoveAsync(string[] keys);
    }
}
