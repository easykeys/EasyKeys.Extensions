using Bet.Extensions.Testing.Logging;

using EasyKeys.Extensions.Dapper.UnitTest.Entities;
using EasyKeys.Extensions.Data.Dapper.Options;
using EasyKeys.Extensions.Data.Dapper.Repositories;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Xunit;
using Xunit.Abstractions;

namespace EasyKeys.Extensions.Dapper.UnitTest
{
    public class DependecyInjectionTests
    {
        private readonly ITestOutputHelper _output;

        public DependecyInjectionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Test()
        {
            var dic = new Dictionary<string, string>
            {
                { "ConnectionStrings:ConnectionString", "sql-connection-string" },
            };

            var configBuilder = new ConfigurationBuilder().AddInMemoryCollection(dic);

            var config = configBuilder.Build();

            var services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(config);
            services.AddLogging(x => x.AddXunit(_output));

            services.AddDapperCachedRepository<Vendor>(
                (options, config) =>
                {
                    options.ConnectionString = config["ConnectionStrings:ConnectionString"];
                });
            //.AddDapperCachedRepository<Vendor>("ConnectionStrings:ConnectionString");

            var sp = services.BuildServiceProvider();

            var options = sp.GetRequiredService<IOptionsMonitor<DbOptions>>().Get(nameof(Vendor));
            Assert.Equal("sql-connection-string", options.ConnectionString);

            var repo = sp.GetService<IAsyncRepositoryCache<Vendor>>();
            Assert.NotNull(repo);

            var cache = sp.GetService<IDistributedCache>();
            Assert.NotNull(cache);
        }
    }
}
