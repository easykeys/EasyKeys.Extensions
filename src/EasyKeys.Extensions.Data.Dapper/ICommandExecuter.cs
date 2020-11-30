using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace EasyKeys.Extensions.Data.Dapper
{
    public interface ICommandExecuter
    {
        Task ExecuteAsync(
            Func<IDbConnection, Task> task,
            string? namedOption = "",
            string? connectionString = null,
            CancellationToken cancellationToken = default);

        Task<TReturn> ExecuteAsync<TReturn>(
            Func<IDbConnection, Task<TReturn>> task,
            string? namedOption = "",
            string? connectionString = null,
            CancellationToken cancellationToken = default);
    }
}
