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
        public void AddDapperCachedRepoWithAction()
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
            var sp = services.BuildServiceProvider();
            var options = sp.GetRequiredService<IOptionsMonitor<DbOptions>>().Get("Vendor");
            Assert.Equal("sql-connection-string", options.ConnectionString);

            var repo = sp.GetService<IAsyncRepositoryCache<Vendor>>();
            Assert.NotNull(repo);

            var cache = sp.GetService<IDistributedCache>();
            Assert.NotNull(cache);
        }

        [Fact]
        public void AddDapperCachedRepoWithDbActionAndCacheAction()
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
                setupAction: (x) =>
                {
                    x.ExpirationScanFrequency = TimeSpan.FromMinutes(1.0);
                    x.SizeLimit = 100;
                });

            var sp = services.BuildServiceProvider();
            var options = sp.GetRequiredService<IOptionsMonitor<DbOptions>>().Get("Vendor");
            Assert.Equal("sql-connection-string", options.ConnectionString);

            var repo = sp.GetService<IAsyncRepositoryCache<Vendor>>();
            Assert.NotNull(repo);

            var cache = sp.GetService<IDistributedCache>();
            Assert.NotNull(cache);
        }

        [Fact]
        public void AddDapperCachedRepoWithOutAction()
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

            services.AddDapperCachedRepository<Vendor>("ConnectionStrings:ConnectionString");

            var sp = services.BuildServiceProvider();

            var options = sp.GetRequiredService<IOptionsMonitor<DbOptions>>().Get("Vendor");

            Assert.Equal("sql-connection-string", options.ConnectionString);

            var repo = sp.GetService<IAsyncRepositoryCache<Vendor>>();
            Assert.NotNull(repo);

            var cache = sp.GetService<IDistributedCache>();
            Assert.NotNull(cache);
        }

        [Fact]
        public void AddDapperCachedRepoWithOutActionAndSectionName()
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

            services.AddDapperCachedRepository<Vendor>();

            var sp = services.BuildServiceProvider();

            var options = sp.GetRequiredService<IOptionsMonitor<DbOptions>>().Get("Vendor");

            Assert.Equal("sql-connection-string", options.ConnectionString);

            var repo = sp.GetService<IAsyncRepositoryCache<Vendor>>();
            Assert.NotNull(repo);

            var cache = sp.GetService<IDistributedCache>();
            Assert.NotNull(cache);
        }

        [Fact]
        public void AddDapperCachedRepoWithDbCacheAction()
        {
            var dic = new Dictionary<string, string>
            {
                { "ConnectionStrings:ConnectionString", "sql-connection-string" },
                { "DbCachedOptions:CacheOptions:SlidingExpiration", "00:00:05" },
            };

            var configBuilder = new ConfigurationBuilder().AddInMemoryCollection(dic);

            var config = configBuilder.Build();

            var services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(config);

            services.AddLogging(x => x.AddXunit(_output));

            services.AddChangeTokenOptions<DbCachedOptions>(
                "DbCachedOptions",
                optionName: "Vendor",
                configureAction: sp => { });

            services.AddDapperCachedRepository<Vendor>();

            var sp = services.BuildServiceProvider();

            var options = sp.GetRequiredService<IOptionsMonitor<DbOptions>>().Get("Vendor");

            var cacheOptions = sp.GetRequiredService<IOptionsMonitor<DbCachedOptions>>().Get("Vendor");
            Assert.Equal("sql-connection-string", options.ConnectionString);

            var repo = sp.GetService<IAsyncRepositoryCache<Vendor>>();
            Assert.NotNull(repo);

            var cache = sp.GetService<IDistributedCache>();
            Assert.NotNull(cache);
        }
    }
}
