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
        public async Task GetMissingKeyReturnsNotNullAsync()
        {
            // arrange
            var cache = _serviceProvider.GetDistributedCache();
            var key = "testKey";

            // act
            var result = await cache.GetAsync<Vendor>(key);

            // assert
            Assert.Null(result);
        }

        [Fact]
        public async Task SetAndGetReturnsObjectAsync()
        {
            var cache = _serviceProvider.GetDistributedCache();
            var vendor = new Vendor();
            var key = "testKey";
            var options = _serviceProvider.GetDistributedCacheEntryOptions(null).CurrentValue;

            await cache.SetAsync<Vendor>(key, vendor, options);

            var result = await cache.GetAsync<Vendor>(key);

            Assert.Equal(vendor.Code, result?.Code);
        }

        [Fact]
        public async void SetAndGetMultipleReturnsObjects()
        {
            var cache = _serviceProvider.GetDistributedCache();
            var vendors = new List<Vendor>() { new Vendor(), new Vendor() };
            var key = "testKey";
            var options = _serviceProvider.GetDistributedCacheEntryOptions(null).CurrentValue;

            await cache.SetAsync<Vendor>(key, vendors, options);

            var result = await cache.GetManyAsync<Vendor>(key);

            Assert.Equal(vendors.SelectMany(x => x.Code), result?.SelectMany(x => x.Code));
        }

        [Fact]
        public async Task GetOrCreateReturnsObjectAsync()
        {
            var cache = _serviceProvider.GetDistributedCache();
            var key = "testKey";
            var vendor = new Vendor();
            var options = _serviceProvider.GetDistributedCacheEntryOptions(null).CurrentValue;

            var factory = new Func<Vendor>(() => vendor);

            var result = await cache.GetOrCreateAsync<Vendor>(
                key,
                factory,
                options);

            Assert.Equal(vendor.Code, result.Code);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async void CacheRemovedWhenExceedingSlidingExpiration(int time)
        {
            var cache = _serviceProvider.GetDistributedCache();
            var key = "testKey";
            var vendor = new Vendor();
            var customOptions = new DistributedCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromSeconds(time)
            };

            await cache.SetAsync<Vendor>(key, vendor, customOptions);

            Thread.Sleep(time * 1000);

            var result = await cache.GetAsync<Vendor>(key);

            Assert.Null(result);
        }
    }
}
