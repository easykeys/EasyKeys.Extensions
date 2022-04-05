using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using EasyKeys.Extensions.Data.Dapper.Paging;
using EasyKeys.Extensions.Data.Dapper.Sorting;

namespace EasyKeys.Extensions.Data.Dapper.Repositories
{
    public interface IAsyncRepository<T> where T : BaseEntity
    {
        ICommandExecuter CommandExecuter { get; }

        string TableName { get; }

        Task<bool> DeleteAsync(T data, string? namedOption = default, CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> GetAllAsync(string? namedOption = default, CancellationToken cancellationToken = default);

        Task<T> GetByIdAsync(int id, string? namedOption = default, CancellationToken cancellationToken = default);

        Task<int> InsertAsync(T data, string? namedOption = default, CancellationToken cancellationToken = default);

        Task<bool> UpdateAsync(T data, string? namedOption = default, CancellationToken cancellationToken = default);

        Task<PagedResults<T>> GetAsync(
            PagedRequest pagedRequest,
            object filters,
            List<SortDescriptor>? sortDescriptors = default,
            string? namedOption = default,
            CancellationToken cancellationToken = default);

        Task<PagedResults<T>> GetAsync(
            PagedRequest pagedRequest,
            string filters,
            List<SortDescriptor>? sortDescriptors = default,
            string? namedOption = default,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> GetAsync(
          object filters,
          List<SortDescriptor>? sortDescriptors = default,
          string? namedOption = default,
          CancellationToken cancellationToken = default);
    }
}
