using System.Data;

using EasyKeys.Extensions.Dapper.UnitTest.Entities;
using EasyKeys.Extensions.Data.Dapper;
using EasyKeys.Extensions.Data.Dapper.Paging;
using EasyKeys.Extensions.Data.Dapper.Repositories;

using Moq;

using Xunit;

namespace EasyKeys.Extensions.Dapper.UnitTest.Repositories
{
    public class DapperRepositoryTests
    {
        [Fact]
        public async Task GetByIdAsync_Should_Return_Single_Entity()
        {
            // arrange
            var mockCmdExe = new Mock<ICommandExecuter>();
            var vendor = new Vendor();
            mockCmdExe.Setup(x => x.ExecuteAsync(
                It.IsAny<Func<IDbConnection, Task<Vendor>>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(vendor)
                .Verifiable();

            var dapperRepository = new DapperRepository<Vendor>(mockCmdExe.Object);

            // act
            var result = await dapperRepository.GetByIdAsync(1);

            // assert
            Assert.IsType<Vendor>(result);
            Assert.Equal("Manufacturer", dapperRepository.TableName);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Multiple_EntitiesAsync()
        {
            // arrange
            var mockCmdExe = new Mock<ICommandExecuter>();
            var vendors = new List<Vendor>();
            mockCmdExe.Setup(x => x.ExecuteAsync(
                It.IsAny<Func<IDbConnection, Task<IEnumerable<Vendor>>>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(vendors)
                .Verifiable();

            var dapperRepository = new DapperRepository<Vendor>(mockCmdExe.Object);

            // act
            var result = await dapperRepository.GetAllAsync();

            // assert
            Assert.IsType<List<Vendor>>(result);
            Assert.Equal("Manufacturer", dapperRepository.TableName);
        }

        [Fact]
        public async Task InsertAsync_Should_Return_Int_0()
        {
            // arrange
            var mockCmdExe = new Mock<ICommandExecuter>();
            var vendor = new Vendor();
            mockCmdExe.Setup(x => x.ExecuteAsync(
                It.IsAny<Func<IDbConnection, Task<int>>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(0)
                .Verifiable();

            var dapperRepository = new DapperRepository<Vendor>(mockCmdExe.Object);

            // act
            var result = await dapperRepository.InsertAsync(vendor);

            // assert
            Assert.IsType<int>(result);
            Assert.Equal(0, result);
            Assert.Equal("Manufacturer", dapperRepository.TableName);
        }

        [Fact]
        public async Task UpdateDeleteAsync_Should_True()
        {
            // arrange
            var mockCmdExe = new Mock<ICommandExecuter>();
            var vendor = new Vendor();
            mockCmdExe.Setup(x => x.ExecuteAsync(
                It.IsAny<Func<IDbConnection, Task<bool>>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(true)
                .Verifiable();

            var dapperRepository = new DapperRepository<Vendor>(mockCmdExe.Object);

            // act
            var updateResult = await dapperRepository.UpdateAsync(vendor);
            var deleteResult = await dapperRepository.DeleteAsync(vendor);

            // assert
            Assert.IsType<bool>(updateResult);
            Assert.True(updateResult);
            Assert.IsType<bool>(deleteResult);
            Assert.True(deleteResult);
            Assert.Equal("Manufacturer", dapperRepository.TableName);
        }

        [Fact]
        public async Task GetAsync_Should_Return_PagedResults()
        {
            // arrange
            var mockCmdExe = new Mock<ICommandExecuter>();
            var vendor = new Vendor();
            mockCmdExe.Setup(x => x.ExecuteAsync(
                It.IsAny<Func<IDbConnection, Task<PagedResults<Vendor>>>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(new PagedResults<Vendor>())
                .Verifiable();

            var dapperRepository = new DapperRepository<Vendor>(mockCmdExe.Object);

            // act
            var result = await dapperRepository.GetAsync(new PagedRequest(), new object());

            // assert
            Assert.IsType<PagedResults<Vendor>>(result);
            Assert.Equal("Manufacturer", dapperRepository.TableName);
        }

        [Fact]
        public async Task GetAsync_Should_Return_List_Of_Vendors()
        {
            // arrange
            var mockCmdExe = new Mock<ICommandExecuter>();
            var vendors = new List<Vendor>();
            mockCmdExe.Setup(x => x.ExecuteAsync(
                It.IsAny<Func<IDbConnection, Task<IEnumerable<Vendor>>>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(vendors)
                .Verifiable();

            var dapperRepository = new DapperRepository<Vendor>(mockCmdExe.Object);

            // act
            var result = await dapperRepository.GetAsync(new object());

            // assert
            Assert.IsType<List<Vendor>>(result);
            Assert.Equal("Manufacturer", dapperRepository.TableName);
        }
    }
}
