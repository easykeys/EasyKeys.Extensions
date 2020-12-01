using System.Collections.Generic;
using System.Threading.Tasks;

using DataWorkerSample.Entities;

using EasyKeys.Extensions.Data.Dapper.Paging;
using EasyKeys.Extensions.Data.Dapper.Repositories;
using EasyKeys.Extensions.Data.Dapper.Sorting;

namespace DataWorkerSample.Services
{
    public class EmailService
    {
        private readonly IAsyncRepository<EmailLogEntity> _asyncRepository;

        public EmailService(IAsyncRepository<EmailLogEntity> asyncRepository)
        {
            _asyncRepository = asyncRepository;
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

            var result = _asyncRepository.GetAsync(new PagedRequest { PageNo = pageNo, PageSize = pageSize }, new { FromEmail = "info@easykeys.com" }, sorting);

            return result;
        }
    }
}
