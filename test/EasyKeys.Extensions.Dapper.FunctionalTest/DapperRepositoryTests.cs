using Bet.Extensions.Testing.Logging;

using EasyKeys.Extensions.Dapper.UnitTest.Entities;
using EasyKeys.Extensions.Data.Dapper.Paging;
using EasyKeys.Extensions.Data.Dapper.Repositories;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Xunit;
using Xunit.Abstractions;

namespace EasyKeys.Extensions.Dapper.FunctionalTest
{
    public class DapperRepositoryTests
    {
        private readonly ITestOutputHelper _output;

        public DapperRepositoryTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task Successfully_Updates_Entities()
        {
            var services = new ServiceCollection();
            var dic = new Dictionary<string, string>
            {
                { "AzureVault:BaseUrl", "https://easykeys1.vault.azure.net/" },
            };

            services.AddLogging(x => x.AddXunit(_output));

            var configBuilder = new ConfigurationBuilder().AddInMemoryCollection(dic);
            configBuilder.AddAzureKeyVault(hostingEnviromentName: "Development", usePrefix: true);

            var config = configBuilder.Build();

            services.AddSingleton<IConfiguration>(config);

            services.AddDapperRepository<Vendor>(
                 namedOption: nameof(Vendor),
                 configure: (o, c) => o.ConnectionString = c["ConnectionStrings:Main:ConnectionString"]);

            var sp = services.BuildServiceProvider();

            var repo = sp.GetRequiredService<IAsyncRepository<Vendor>>();

            var result = await repo.GetAsync(new PagedRequest() { PageNo = 1, PageSize = 50 }, "ManufacturerId < 100", null, nameof(Vendor));

            var resultItems = result.Items;

            Assert.True(resultItems.Any());

            resultItems.ToList().ForEach(x => x.ContactEmail = "Brandon Moffett Test");

            await repo.UpdateAsync(resultItems, nameof(Vendor));

            var resultTest = await repo.GetAsync(new PagedRequest() { PageNo = 1, PageSize = 50 }, "ContactEmail = 'Brandon Moffett Test'", null, nameof(Vendor));

            Assert.Contains(resultTest.Items, x => x.ContactEmail == "Brandon Moffett Test");
        }
    }
}
