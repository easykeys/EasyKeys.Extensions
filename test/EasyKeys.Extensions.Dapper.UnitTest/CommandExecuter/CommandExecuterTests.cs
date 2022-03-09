using System.Data;

using EasyKeys.Extensions.Dapper.UnitTest.Entities;
using EasyKeys.Extensions.Data.Dapper;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;
using Xunit.Abstractions;

namespace EasyKeys.Extensions.Dapper.UnitTest.CommandExe
{
    public class CommandExecuterTests : IClassFixture<ServiceProviderFixture>
    {
        private ServiceProviderFixture _fixture;
        private ITestOutputHelper _output;

        public CommandExecuterTests(
            ServiceProviderFixture fixture,
            ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }

        [Fact]
        public async void ExecuteAsync_Log_Completed()
        {
            var options = _fixture.GetDbOptions();
            var mockLogger = new Mock<ILogger<CommandExecuter>>();

            mockLogger.Setup(x => x.Log(
               It.IsAny<LogLevel>(),
               It.IsAny<EventId>(),
               It.IsAny<It.IsAnyType>(),
               It.IsAny<Exception>(),
               (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));

            var commandExe = new CommandExecuter(options, mockLogger.Object);
            var mockFunk = new Func<IDbConnection, Task>((x) => Task.CompletedTask);

            await commandExe.ExecuteAsync(
                mockFunk,
                nameof(Vendor));

            mockLogger.Verify(
                x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Exactly(2));
        }
    }
}
