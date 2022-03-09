using EasyKeys.Extensions.Dapper.UnitTest.Entities;

using Microsoft.Extensions.Caching.Distributed;

using Xunit;

namespace EasyKeys.Extensions.Dapper.UnitTest.Cache
{
    public class CacheTests : IClassFixture<ServiceProviderFixture>
    {
        private ServiceProviderFixture _serviceProvider;

        public CacheTests(ServiceProviderFixture serviceProviderFixture)
        {
            _serviceProvider = serviceProviderFixture;
        }

        [Fact]
        public async Task CacheExtention_CRUD_TestsAsync()
        {
            var cache = _serviceProvider.GetDistributedCache();
            var options = _serviceProvider.GetDistributedCacheEntryOptions();

            // check single set/get for string
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
