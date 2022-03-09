using EasyKeys.Extensions.Dapper.UnitTest.Entities;
using EasyKeys.Extensions.Dapper.UnitTest.Mocks;
using EasyKeys.Extensions.Data.Dapper;
using EasyKeys.Extensions.Data.Dapper.Options;
using EasyKeys.Extensions.Data.Dapper.Repositories;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Xunit;

namespace EasyKeys.Extensions.Dapper.UnitTest.Repositories;

public class DapperRepositoryCacheTests
{
    [Fact]
    public async Task GetTest()
    {
        var dic = new Dictionary<string, string>
            {
                { "DbCachedOptions:CacheOptions:SlidingExpiration", "00:00:05" },
            };

        var configBuilder = new ConfigurationBuilder().AddInMemoryCollection(dic);

        var config = configBuilder.Build();

        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(config);

        services.AddChangeTokenOptions<DbCachedOptions>(
            "DbCachedOptions",
            optionName: "Vendor",
            configureAction: sp => { });

        services.AddTransient<ICommandExecuter, MockCommandExecuter>();

        services.AddTransient<IAsyncRepositoryCache<Vendor>, DapperRepositoryCache<Vendor>>();

        services.AddDistributedMemoryCache();

        var sp = services.BuildServiceProvider();

        var options = sp.GetRequiredService<IOptionsMonitor<DbCachedOptions>>().Get("Vendor");

        Assert.Equal(TimeSpan.Parse("00:00:05"), options.CacheOptions.SlidingExpiration);

        var repo = sp.GetRequiredService<IAsyncRepositoryCache<Vendor>>();

        var result = await repo.GetByIdAsync(1, namedOption: "Vendor");
        Assert.NotNull(result);
    }
}
