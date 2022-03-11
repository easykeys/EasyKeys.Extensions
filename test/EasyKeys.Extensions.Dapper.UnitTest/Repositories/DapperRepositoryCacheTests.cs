using System.Data;
using System.Runtime.Serialization.Formatters.Binary;

using EasyKeys.Extensions.Dapper.UnitTest.Entities;
using EasyKeys.Extensions.Dapper.UnitTest.Mocks;
using EasyKeys.Extensions.Data.Dapper;
using EasyKeys.Extensions.Data.Dapper.Options;
using EasyKeys.Extensions.Data.Dapper.Paging;
using EasyKeys.Extensions.Data.Dapper.Repositories;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Moq;

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

    [Fact]
    public async Task DeleteAsyncRemovesFromCacheAndDbAsync()
    {
        var mockOptions = new Mock<IOptionsMonitor<DbCachedOptions>>();
        var mockCache = new Mock<IDistributedCache>();
        var mockCmdExe = new MockCommandExecuter();
        var vendor = new Vendor();
        mockCache.Setup(x => x.RemoveAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>())).
            Verifiable();

        var dapperRepoCache = new DapperRepositoryCache<Vendor>(
            mockOptions.Object,
            mockCache.Object,
            mockCmdExe);

        var result = await dapperRepoCache.DeleteAsync(vendor);

        Assert.Equal("Manufacturer", dapperRepoCache.TableName);
        mockCache.Verify(
            x => x.RemoveAsync(
            It.Is<string>(x =>
                x.Contains($"{dapperRepoCache.TableName}")),
            It.IsAny<CancellationToken>()),
            Times.Exactly(1));

        Assert.False(result);
    }

    [Fact]
    public async void GetAllAsyncGetsReturnsCachedItem()
    {
        var mockOptions = new Mock<IOptionsMonitor<DbCachedOptions>>();
        var mockCache = new Mock<IDistributedCache>();
        var mockCmdExe = new Mock<ICommandExecuter>();
        var vendors = new List<Vendor>();

        using var stream = new MemoryStream();
        new BinaryFormatter()?.Serialize(stream, vendors);

        mockCache.Setup(
            x => x.GetAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>())).
            ReturnsAsync(stream.ToArray()).
            Verifiable();

        mockCmdExe.Setup(x => x.ExecuteAsync(
            It.IsAny<Func<IDbConnection, Task<IEnumerable<Vendor>>>>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(vendors)
            .Verifiable();

        var dapperRepoCache = new DapperRepositoryCache<Vendor>(
            mockOptions.Object,
            mockCache.Object,
            mockCmdExe.Object);

        var result = await dapperRepoCache.GetAllAsync();

        mockCache.Verify(
            x => x.GetAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()),
            Times.Exactly(1));

        mockCmdExe.Verify(
            x => x.ExecuteAsync(
            It.IsAny<Func<IDbConnection, Task<IEnumerable<Vendor>>>>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()),
            Times.Never);

        Assert.IsType<List<Vendor>>(result);
    }

    [Fact]
    public async Task GetAllAsyncReturnsDbResultAsync()
    {
        var mockOptions = new Mock<IOptionsMonitor<DbCachedOptions>>();
        var mockCache = new Mock<IDistributedCache>();
        var mockCmdExe = new Mock<ICommandExecuter>();
        var vendors = new List<Vendor>() { new Vendor(), new Vendor() };

        using var stream = new MemoryStream();
        new BinaryFormatter()?.Serialize(stream, vendors);

        mockCache.Setup(
            x => x.GetAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>())).
            ReturnsAsync(default(byte[])).
            Verifiable();

        mockCmdExe.Setup(x => x.ExecuteAsync(
            It.IsAny<Func<IDbConnection, Task<IEnumerable<Vendor>>>>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(vendors)
            .Verifiable();

        mockOptions.Setup(x => x.Get(
            It.IsAny<string>()))
            .Returns(new DbCachedOptions());

        var dapperRepoCache = new DapperRepositoryCache<Vendor>(
            mockOptions.Object,
            mockCache.Object,
            mockCmdExe.Object);

        var result = await dapperRepoCache.GetAllAsync();

        mockCache.Verify(
            x => x.GetAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()),
            Times.Exactly(1));

        mockCmdExe.Verify(
            x => x.ExecuteAsync(
            It.IsAny<Func<IDbConnection, Task<IEnumerable<Vendor>>>>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()),
            Times.Exactly(1));

        Assert.IsType<List<Vendor>>(result);
    }

    [Fact]
    public async Task GetAsyncReturnsPagedResultsTAsync()
    {
        var mockOptions = new Mock<IOptionsMonitor<DbCachedOptions>>();
        var mockCache = new Mock<IDistributedCache>();
        var mockCmdExe = new Mock<ICommandExecuter>();
        var pagedResults = new PagedResults<Vendor>();

        mockCache.Setup(
            x => x.GetAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>())).
            Verifiable();

        mockCmdExe.Setup(x => x.ExecuteAsync(
            It.IsAny<Func<IDbConnection, Task<PagedResults<Vendor>>>>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResults)
            .Verifiable();

        var dapperRepoCache = new DapperRepositoryCache<Vendor>(
            mockOptions.Object,
            mockCache.Object,
            mockCmdExe.Object);

        var result = await dapperRepoCache.GetAsync(
            new PagedRequest(),
            new object());

        mockCache.Verify(
            x => x.GetAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()),
            Times.Never);

        mockCmdExe.Verify(
            x => x.ExecuteAsync(
            It.IsAny<Func<IDbConnection, Task<PagedResults<Vendor>>>>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()),
            Times.Exactly(1));

        Assert.IsType<PagedResults<Vendor>>(result);
    }

    [Fact]
    public async void GetByIdAsyncGetsReturnsCachedItem()
    {
        var mockOptions = new Mock<IOptionsMonitor<DbCachedOptions>>();
        var mockCache = new Mock<IDistributedCache>();
        var mockCmdExe = new Mock<ICommandExecuter>();
        var vendor = new Vendor();

        using var stream = new MemoryStream();
        new BinaryFormatter()?.Serialize(stream, vendor);

        mockCache.Setup(
            x => x.GetAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>())).
            ReturnsAsync(stream.ToArray()).
            Verifiable();

        mockCmdExe.Setup(x => x.ExecuteAsync(
            It.IsAny<Func<IDbConnection, Task<Vendor>>>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(vendor)
            .Verifiable();

        var dapperRepoCache = new DapperRepositoryCache<Vendor>(
            mockOptions.Object,
            mockCache.Object,
            mockCmdExe.Object);

        var result = await dapperRepoCache.GetByIdAsync(1);

        mockCache.Verify(
            x => x.GetAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()),
            Times.Exactly(1));

        mockCmdExe.Verify(
            x => x.ExecuteAsync(
            It.IsAny<Func<IDbConnection, Task<Vendor>>>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()),
            Times.Never);

        Assert.IsType<Vendor>(result);
    }

    [Fact]
    public async Task GetByIdAsyncReturnsDbResultAsync()
    {
        var mockOptions = new Mock<IOptionsMonitor<DbCachedOptions>>();
        var mockCache = new Mock<IDistributedCache>();
        var mockCmdExe = new Mock<ICommandExecuter>();
        var vendor = new Vendor();

        using var stream = new MemoryStream();
        new BinaryFormatter()?.Serialize(stream, vendor);

        mockCache.Setup(
            x => x.GetAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>())).
            ReturnsAsync(default(byte[])).
            Verifiable();

        mockCmdExe.Setup(x => x.ExecuteAsync(
            It.IsAny<Func<IDbConnection, Task<Vendor>>>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(vendor)
            .Verifiable();

        mockOptions.Setup(x => x.Get(
            It.IsAny<string>()))
            .Returns(new DbCachedOptions());

        var dapperRepoCache = new DapperRepositoryCache<Vendor>(
            mockOptions.Object,
            mockCache.Object,
            mockCmdExe.Object);

        var result = await dapperRepoCache.GetByIdAsync(2);

        mockCache.Verify(
            x => x.GetAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()),
            Times.Exactly(1));

        mockCmdExe.Verify(
            x => x.ExecuteAsync(
            It.IsAny<Func<IDbConnection, Task<Vendor>>>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()),
            Times.Exactly(1));

        Assert.IsType<Vendor>(result);
    }


    [Fact]
    public async Task InsertAsyncReturnsTaskCompleted()
    {
        var mockOptions = new Mock<IOptionsMonitor<DbCachedOptions>>();
        var mockCache = new Mock<IDistributedCache>();
        var mockCmdExe = new MockCommandExecuter();
        var pagedResults = new PagedResults<Vendor>();

        mockCache.Setup(
            x => x.GetAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>())).
            Verifiable();

        var dapperRepoCache = new DapperRepositoryCache<Vendor>(
            mockOptions.Object,
            mockCache.Object,
            mockCmdExe);

        var result = await dapperRepoCache.InsertAsync(new Vendor());

        mockCache.Verify(
            x => x.GetAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()),
            Times.Never);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task UpdateAsyncReturnsTaskCompleted()
    {
        var mockOptions = new Mock<IOptionsMonitor<DbCachedOptions>>();
        var mockCache = new Mock<IDistributedCache>();
        var mockCmdExe = new MockCommandExecuter();
        var pagedResults = new PagedResults<Vendor>();

        mockCache.Setup(
            x => x.GetAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>())).
            Verifiable();

        var dapperRepoCache = new DapperRepositoryCache<Vendor>(
            mockOptions.Object,
            mockCache.Object,
            mockCmdExe);

        var result = await dapperRepoCache.InsertAsync(new Vendor());

        mockCache.Verify(
            x => x.GetAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()),
            Times.Never);

        Assert.Equal(0, result);
    }
}
