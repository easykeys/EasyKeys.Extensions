using System.Data;

using EasyKeys.Extensions.Dapper.UnitTest.Entities;
using EasyKeys.Extensions.Data.Dapper;

namespace EasyKeys.Extensions.Dapper.UnitTest.Mocks;

public class MockCommandExecuter : ICommandExecuter
{
    public Task ExecuteAsync(
        Func<IDbConnection, Task> task,
        string? namedOption = "",
        string? connectionString = null,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<TReturn> ExecuteAsync<TReturn>(
        Func<IDbConnection, Task<TReturn>> task,
        string? namedOption = "",
        string? connectionString = null,
        CancellationToken cancellationToken = default)
    {
        var t = typeof(TReturn);
        if (t == typeof(IEnumerable<Vendor>))
        {
            var test = (TReturn)Activator.CreateInstance(t);
            return Task.FromResult(test);
        }

        var v = (TReturn)Activator.CreateInstance(t);
        return Task.FromResult(v);
    }
}
