namespace EasyKeys.Extensions.Data.Dapper.Repositories
{
    public interface IAsyncRepositoryCache<T> : IAsyncRepository<T> where T : BaseEntity, new()
    {
    }
}
