using Bet.Extensions.Testing.Logging;

using EasyKeys.Extensions.Dapper.UnitTest.Entities;
using EasyKeys.Extensions.Data.Dapper.Options;
using EasyKeys.Extensions.Data.Dapper.Paging;
using EasyKeys.Extensions.Data.Dapper.Repositories;
using EasyKeys.Extensions.Data.Dapper.Sorting;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            var services = new ServiceCollection();
            var dic = new Dictionary<string, string>
            {
                { "AzureVault:BaseUrl", "https://easykeys1.vault.azure.net/" },
                { "DbCachedOptions:CacheOptions:SlidingExpiration", "01:00:00" },
            };

            services.AddLogging(x => x.AddXunit(_output));

            var configBuilder = new ConfigurationBuilder().AddInMemoryCollection(dic);
            configBuilder.AddAzureKeyVault(hostingEnviromentName: "Development", usePrefix: true);

            var config = configBuilder.Build();

            services.AddSingleton<IConfiguration>(config);

            services.AddChangeTokenOptions<DbCachedOptions>(
                "DbCachedOptions",
                optionName: "Vendor",
                configureAction: sp => { });

            services.AddDapperCachedRepository<Vendor>("ConnectionStrings:Main:ConnectionString");

            var sp = services.BuildServiceProvider();

            var options = sp.GetRequiredService<IOptionsMonitor<DbOptions>>().Get("Vendor");

            var cacheOptions = sp.GetRequiredService<IOptionsMonitor<DbCachedOptions>>().Get("Vendor");

            Assert.Equal(TimeSpan.Parse("01:00:00"), cacheOptions.CacheOptions.SlidingExpiration);

            var repo = sp.GetRequiredService<IAsyncRepositoryCache<Vendor>>();
            Assert.NotNull(repo);

            var result = await repo.GetByIdAsync(10, namedOption: "Vendor");
            Assert.NotNull(result);

            var cacheResult = await repo.GetByIdAsync(10, namedOption: "Vendor");

            Assert.NotNull(cacheResult);
        }

        [Fact]
        public async Task GetAllAsyncReturnsEntityThenCache()
        {
            var services = new ServiceCollection();
            var dic = new Dictionary<string, string>
            {
                { "AzureVault:BaseUrl", "https://easykeys1.vault.azure.net/" },
                { "DbCachedOptions:CacheOptions:SlidingExpiration", "00:00:05" },
            };

            services.AddLogging(x => x.AddXunit(_output));

            var configBuilder = new ConfigurationBuilder().AddInMemoryCollection(dic);
            configBuilder.AddAzureKeyVault(hostingEnviromentName: "Development", usePrefix: true);

            var config = configBuilder.Build();

            services.AddSingleton<IConfiguration>(config);

            services.AddChangeTokenOptions<DbCachedOptions>(
                "DbCachedOptions",
                optionName: "Vendor",
                configureAction: sp => { });

            services.AddDapperCachedRepository<Vendor>("ConnectionStrings:Main:ConnectionString");

            var sp = services.BuildServiceProvider();

            var repo = sp.GetRequiredService<IAsyncRepositoryCache<Vendor>>();

            var result = await repo.GetAllAsync(nameof(Vendor));

            Assert.True(result.Any());

            var result2 = await repo.GetAllAsync(nameof(Vendor));

            Assert.True(result2.Any());
        }

        [Fact]
        public async Task GetAsyncPagedRequestReturnsCorrectPageSizeWithSortingFilter()
        {
            var services = new ServiceCollection();
            var dic = new Dictionary<string, string>
            {
                { "AzureVault:BaseUrl", "https://easykeys1.vault.azure.net/" },
                { "DbCachedOptions:CacheOptions:SlidingExpiration", "01:00:00" },
            };

            services.AddLogging(x => x.AddXunit(_output));

            var configBuilder = new ConfigurationBuilder().AddInMemoryCollection(dic);
            configBuilder.AddAzureKeyVault(hostingEnviromentName: "Development", usePrefix: true);

            var config = configBuilder.Build();

            services.AddSingleton<IConfiguration>(config);

            services.AddChangeTokenOptions<DbCachedOptions>(
                "DbCachedOptions",
                optionName: "Vendor",
                configureAction: sp => { });

            services.AddDapperCachedRepository<Vendor>("ConnectionStrings:Main:ConnectionString");

            var sp = services.BuildServiceProvider();

            var pagedRequest = new PagedRequest() { PageNo = 1, PageSize = 25 };

            var sortDiscriptors = new List<SortDescriptor>()
            {
                new SortDescriptor()
                {
                    Direction = SortingDirection.Ascending,
                    Field = "HtmmapName"
                }
            };
            var repo = sp.GetRequiredService<IAsyncRepositoryCache<Vendor>>();

            var result = await repo.GetAsync(pagedRequest, new object(), sortDiscriptors, nameof(Vendor));

            Assert.Equal(pagedRequest.PageSize, result.Items.Count());
        }

        [Fact]
        public async Task GetAsyncPagedRequestThrowsSqlExceptionWhenSortDescriptorFieldIsEmptyAsync()
        {
            var services = new ServiceCollection();
            var dic = new Dictionary<string, string>
            {
                { "AzureVault:BaseUrl", "https://easykeys1.vault.azure.net/" },
                { "DbCachedOptions:CacheOptions:SlidingExpiration", "01:00:00" },
            };

            services.AddLogging(x => x.AddXunit(_output));

            var configBuilder = new ConfigurationBuilder().AddInMemoryCollection(dic);
            configBuilder.AddAzureKeyVault(hostingEnviromentName: "Development", usePrefix: true);

            var config = configBuilder.Build();

            services.AddSingleton<IConfiguration>(config);

            services.AddChangeTokenOptions<DbCachedOptions>(
                "DbCachedOptions",
                optionName: "Vendor",
                configureAction: sp => { });

            services.AddDapperCachedRepository<Vendor>("ConnectionStrings:Main:ConnectionString");

            var sp = services.BuildServiceProvider();

            var pagedRequest = new PagedRequest() { PageNo = 1, PageSize = 25 };

            var sortDiscriptors = new List<SortDescriptor>()
            {
                new SortDescriptor()
                {
                    Direction = SortingDirection.Ascending
                }
            };
            var repo = sp.GetRequiredService<IAsyncRepositoryCache<Vendor>>();

            var ex = await Assert.ThrowsAsync<Exception>(() => repo.GetAsync(pagedRequest, new object(), sortDiscriptors, nameof(Vendor)));
        }
    }
}
