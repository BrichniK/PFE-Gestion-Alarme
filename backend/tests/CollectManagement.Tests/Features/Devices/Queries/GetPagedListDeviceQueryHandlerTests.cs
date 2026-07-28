using CollectManagement.Application.Features.Devices.Queries.GetPagedListDevice;
using CollectManagement.Application.Interfaces.Repositories.Devices;
using FluentAssertions;
using MapsterMapper;
using Moq;

namespace CollectManagement.Tests.Features.Devices;

public class GetPagedListDeviceQueryHandlerTests
{
    [Fact]
    public async Task HandleShouldReturnPagedResult()
    {
        // Arrange
        var repository = new Mock<IDeviceRepository>();
        var mapper = new Mock<IMapper>();

        repository
            .Setup(x => x.GetPagedListAsync(
                null,
                null,
                null,
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (
                    new List<CollectManagement.Domain.Devices.Device>(),
                    0
                ));


        var handler =
            new GetPagedListDeviceQueryHandler(
                repository.Object,
                mapper.Object);


        var query = new GetPagedListDeviceQuery(
            null,
            null,
            null,
            1,
            10);


        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);


        // Assert
        result.Should().NotBeNull();


        repository.Verify(
            x => x.GetPagedListAsync(
                null,
                null,
                null,
                1,
                10,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}