using DataWorkerSample.Entities;

using EasyKeys.Extensions.Data.Dapper.Paging;
using EasyKeys.Extensions.Data.Dapper.Repositories;
using EasyKeys.Extensions.Data.Dapper.Sorting;

namespace DataWorkerSample.Services;

public class EmailService
{
    private readonly IAsyncRepositoryCache<EmailLogEntity> _cachedRepo;
    private readonly IAsyncRepository<EmailLogEntity> _repo;

    public EmailService(
        IAsyncRepositoryCache<EmailLogEntity> cachedRepo,
        IAsyncRepository<EmailLogEntity> repo)
    {
        _cachedRepo = cachedRepo;
        _repo = repo;
    }

    public Task<PagedResults<EmailLogEntity>> GetAsync(int pageNo = 1, int pageSize = 100)
    {
        var sorting = new List<SortDescriptor>()
        {
            new SortDescriptor
            {
                Direction = SortingDirection.Descending,
                Field = "InsertDateTime",
            },
        };

        var result = _repo.GetAsync(new PagedRequest { PageNo = pageNo, PageSize = pageSize }, new { FromEmail = "info@easykeys.com" }, sorting);

        return result;
    }

    public Task<PagedResults<EmailLogEntity>> GetCachedAsync(int pageNo = 1, int pageSize = 100)
    {
        var sorting = new List<SortDescriptor>()
        {
            new SortDescriptor
            {
                Direction = SortingDirection.Descending,
                Field = "InsertDateTime",
            },
        };

        var result = _cachedRepo.GetAsync(new PagedRequest { PageNo = pageNo, PageSize = pageSize }, new { FromEmail = "info@easykeys.com" }, sorting);

        return result;
    }

    public Task<int> InsertAsync(EmailLogEntity entity, CancellationToken cancellationToken = default)
    {
        return _repo.InsertAsync(data: entity, cancellationToken: cancellationToken);
    }
}
