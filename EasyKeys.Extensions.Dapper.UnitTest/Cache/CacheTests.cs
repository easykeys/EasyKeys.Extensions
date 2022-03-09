using EasyKeys.Extensions.Dapper.UnitTest.Entities;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Xunit;

namespace EasyKeys.Extensions.Dapper.UnitTest.Cache
{
    public class CacheTests : IClassFixture<ServiceProviderFixter>
    {

        public CacheTests()
        {
        }

        [Fact]
        public async Task CacheExtention_Operations_TestAsync()
        {
            // TODO : move this somewhere else.
            var services = new ServiceCollection();
            var dic = new Dictionary<string, string>
            {
                { "AzureVault:BaseUrl", "https://easykeys1.vault.azure.net/" },
            };

            var configBuilder = new ConfigurationBuilder().AddInMemoryCollection(dic);

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

            var sp = services.BuildServiceProvider();

            var cache = sp.GetRequiredService<IDistributedCache>();
            var options = sp.GetRequiredService<IOptionsMonitor<DistributedCacheEntryOptions>>();

            // check singe set/get for string
            var key = Guid.NewGuid().ToString();
            var stringTest = "stringTest";
            await IDistributedCacheExtensions.SetAsync(
                cache,
                key,
                stringTest,
                options.CurrentValue);

            var result = await IDistributedCacheExtensions.GetAsync(
                cache,
                key);

            Assert.Equal(stringTest, result);

            // check single set/get for T : baseEntity
            var keyObject = Guid.NewGuid().ToString();
            var vendor = new Vendor();
            await IDistributedCacheExtensions.SetAsync<Vendor>(
                cache,
                keyObject,
                vendor,
                options.CurrentValue);

            var cacheVendor = await IDistributedCacheExtensions.GetAsync<Vendor>(
                cache,
                keyObject);

            Assert.Equal(vendor.Code, cacheVendor?.Code);

            // check many set/get for T : baseEntity
            var keyObjects = Guid.NewGuid().ToString();
            var vendors = new List<Vendor>() { new Vendor(), new Vendor() };
            await IDistributedCacheExtensions.SetAsync(
                cache,
                keyObjects,
                vendors,
                options.CurrentValue);

            var cacheVendors = await IDistributedCacheExtensions.GetManyAsync<Vendor>(
                cache,
                keyObjects);

            Assert.Equal(vendors.LastOrDefault()?.Code, cacheVendors?.LastOrDefault()?.Code);

            // check setOrCreate for string
            var keyGetOrCreateString = Guid.NewGuid().ToString();
            var testGetOrCreate = new Vendor();
            var factoryTest = new Func<Vendor>(() => testGetOrCreate);
            await IDistributedCacheExtensions.GetOrCreateAsync<Vendor>(
                cache,
                keyGetOrCreateString,
                factoryTest,
                options.CurrentValue);

            var cacheGetVendor = await IDistributedCacheExtensions.GetAsync<Vendor>(
                cache,
                keyGetOrCreateString);

            Assert.Equal(testGetOrCreate.Code, cacheGetVendor?.Code);
        }
    }
}
