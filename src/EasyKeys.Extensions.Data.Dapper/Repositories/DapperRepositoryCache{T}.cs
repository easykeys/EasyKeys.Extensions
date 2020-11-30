using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using EasyKeys.Extensions.Data.Dapper.Options;
using EasyKeys.Extensions.Data.Dapper.Paging;
using EasyKeys.Extensions.Data.Dapper.Sorting;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace EasyKeys.Extensions.Data.Dapper.Repositories
{
    public class DapperRepositoryCache<T> : DapperRepository<T>, IAsyncRepositoryCache<T> where T : AuditableEntity, new()
    {
        private readonly IOptionsMonitor<DbCachedOptions> _optionsMonitor;
        private readonly IDistributedCache _cache;

        public DapperRepositoryCache(
            IOptionsMonitor<DbCachedOptions> optionsMonitor,
            IDistributedCache cache,
            ICommandExecuter commandExecuter) : base(commandExecuter)
        {
            _optionsMonitor = optionsMonitor;
            _cache = cache;
        }

        public override async Task<bool> DeleteAsync(T data, string? namedOption = null, CancellationToken cancellationToken = default)
        {
            var key = $"{TableName}-{data.Code}";

            await _cache.RemoveAsync(key, cancellationToken);

            return await base.DeleteAsync(data, namedOption, cancellationToken);
        }

        public override async Task<IEnumerable<T>> GetAllAsync(string? namedOption = null, CancellationToken cancellationToken = default)
        {
            var cachedItem = await _cache.GetManyAsync<T>(TableName);

            if (cachedItem != null)
            {
                return cachedItem;
            }

            var result = await base.GetAllAsync(namedOption, cancellationToken);

            var options = _optionsMonitor.Get(namedOption);

            await _cache.SetAsync<T>(TableName, result, options.CacheOptions);

            return await base.GetAllAsync(namedOption, cancellationToken);
        }

        public override async Task<PagedResults<T>> GetAsync(
            PagedRequest pagedRequest,
            object filters,
            List<SortDescriptor>? sortDescriptors = null,
            string? namedOption = null,
            CancellationToken cancellationToken = default)
        {
            return await base.GetAsync(pagedRequest, filters, sortDescriptors, namedOption, cancellationToken);
        }

        public override async Task<T> GetByIdAsync(int id, string? namedOption = null, CancellationToken cancellationToken = default)
        {
            var key = $"{TableName}-{id}";

            var cachedItem = await _cache.GetAsync<T>(key, cancellationToken);

            if (cachedItem != null)
            {
                return cachedItem;
            }

            var result = await base.GetByIdAsync(id, namedOption, cancellationToken);

            var options = _optionsMonitor.Get(namedOption);

            await _cache.SetAsync<T>(key, result, options.CacheOptions, cancellationToken);

            return result;
        }

        public override Task<int> InsertAsync(T data, string? namedOption = null, CancellationToken cancellationToken = default)
        {
            return base.InsertAsync(data, namedOption, cancellationToken);
        }

        public override Task<bool> UpdateAsync(T data, string? namedOption = null, CancellationToken cancellationToken = default)
        {
            return base.UpdateAsync(data, namedOption, cancellationToken);
        }
    }
}
