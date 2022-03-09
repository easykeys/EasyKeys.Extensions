using Bet.Extensions.Testing.Logging;

using EasyKeys.Extensions.Dapper.UnitTest.Entities;
using EasyKeys.Extensions.Data.Dapper.Options;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Xunit.Abstractions;

namespace EasyKeys.Extensions.Dapper.UnitTest
{
    public class ServiceProviderFixture
    {
        private readonly IServiceProvider? _serviceProvider;

        public ServiceProviderFixture()
        {
            var services = new ServiceCollection();
            var dic = new Dictionary<string, string>
            {
                { "AzureVault:BaseUrl", "https://easykeys1.vault.azure.net/" },
            };

            var configBuilder = new ConfigurationBuilder().AddInMemoryCollection(dic);
            configBuilder.AddAzureKeyVault(hostingEnviromentName: "Development", usePrefix: true);

            var config = configBuilder.Build();

            services.AddSingleton<IConfiguration>(config);

            services.AddDbConnection(
                "ConnectionStrings:Main:ConnectionString",
                optionName: nameof(Vendor),
                configure: (options, config) =>
                {
                    options.ConnectionString = config["ConnectionStrings:Main:ConnectionString"];
                });
            services.AddDapperCachedRepository<Vendor>(
                "ConnectionStrings:Main:ConnectionString",
                namedOption: nameof(Vendor),
                configure: (options, config) =>
                {
                    options.CacheOptions.AbsoluteExpiration = DateTimeOffset.Now.AddHours(1);
                });

            _serviceProvider = services.BuildServiceProvider();
        }

        public IDistributedCache GetDistributedCache()
        {
            return _serviceProvider.GetRequiredService<IDistributedCache>();
        }

        public IOptionsMonitor<DistributedCacheEntryOptions> GetDistributedCacheEntryOptions()
        {
            return _serviceProvider.GetRequiredService<IOptionsMonitor<DistributedCacheEntryOptions>>();
        }

        public IOptionsMonitor<DbOptions> GetDbOptions()
        {
            return _serviceProvider.GetRequiredService<IOptionsMonitor<DbOptions>>();
        }

        public ILoggerFactory CreateLoggerFactory(ITestOutputHelper output)
        {
            var services = new ServiceCollection();
            services.AddLogging(x => x.AddXunit(output));
            var sp = services.BuildServiceProvider();
            return sp.GetRequiredService<ILoggerFactory>();
        }
    }
}
