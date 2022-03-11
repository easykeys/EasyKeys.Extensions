
using Bet.Extensions.Testing.Logging;

using EasyKeys.Extensions.Dapper.UnitTest.Entities;
using EasyKeys.Extensions.Dapper.UnitTest.Mocks;
using EasyKeys.Extensions.Data.Dapper;
using EasyKeys.Extensions.Data.Dapper.Options;
using EasyKeys.Extensions.Data.Dapper.Repositories;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using Xunit;
using Xunit.Abstractions;

namespace EasyKeys.Extensions.Dapper.FunctionalTest
{
    public class DapperRepositoryCacheTests
    {
        private readonly ITestOutputHelper _output;

        public DapperRepositoryCacheTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task GetByIdReturnsEntityAndSavesCache()
        {
            var dic = new Dictionary<string, string>
            {
                { "DbCachedOptions:CacheOptions:SlidingExpiration", "00:00:05" },
                { "ConnectionStrings:ConnectionString", "sql-connection-string" },
            };

            var configBuilder = new ConfigurationBuilder().AddInMemoryCollection(dic);

            var config = configBuilder.Build();

            var services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(config);

            // add this in AddDapperCachedRepository?
            services.AddChangeTokenOptions<DbCachedOptions>(
                "DbCachedOptions",
                optionName: "Vendor",
                configureAction: sp => { });

            services.AddSingleton<IConfiguration>(config);
            services.AddLogging(x => x.AddXunit(_output));

            services.AddDapperCachedRepository<Vendor>();

            // for test purposes
            var found = services.SingleOrDefault(x => x.ServiceType == typeof(ICommandExecuter));

            if (found != null)
            {
                services.Remove(found);
            }

            services.TryAddScoped<ICommandExecuter, MockCommandExecuter>();

            var sp = services.BuildServiceProvider();

            var options = sp.GetRequiredService<IOptionsMonitor<DbOptions>>().Get("Vendor");

            var CacheOptions = sp.GetRequiredService<IOptionsMonitor<DbCachedOptions>>().Get("Vendor");

            Assert.Equal("sql-connection-string", options.ConnectionString);

            Assert.Equal(TimeSpan.Parse("00:00:05"), CacheOptions.CacheOptions.SlidingExpiration);

            var repo = sp.GetRequiredService<IAsyncRepositoryCache<Vendor>>();
            Assert.NotNull(repo);

            var result = await repo.GetByIdAsync(1, namedOption: "Vendor");
            Assert.NotNull(result);

            var cache = sp.GetRequiredService<IDistributedCache>();

            var data = await cache.GetAsync<Vendor>("Manufacturer-1");

            Assert.NotNull(data);
        }
    }
}
