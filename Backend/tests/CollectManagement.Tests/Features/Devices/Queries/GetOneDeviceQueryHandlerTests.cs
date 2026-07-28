using CollectManagement.Application.Features.Devices.Mapping;
using CollectManagement.Application.Features.Devices.Queries.GetOneDevice;
using CollectManagement.Application.Interfaces.Repositories.Devices;
using CollectManagement.Domain.Devices;
using CollectManagement.Domain.Devices.ValueObjects;
using FluentAssertions;
using Mapster;
using MapsterMapper;
using Moq;

namespace CollectManagement.Tests.Features.Devices;

public class GetOneDeviceQueryHandlerTests
{
    [Fact]
    public async Task HandleShouldReturnDeviceWhenExists()
    {
        // Arrange
        var repository = new Mock<IDeviceRepository>();

        // Configuration Mapster
        var config = new TypeAdapterConfig();
        config.Scan(typeof(DeviceMapping).Assembly);

        var mapper = new Mapper(config);


        var deviceId = new DeviceId(Ulid.NewUlid());


        var device = Device.Create(
            deviceId,
            "Device-Test",
            "MAT-001",
            4
        );


        repository
            .Setup(x => x.GetOneAsync(
                deviceId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);


        var handler = new GetOneDeviceQueryHandler(
            repository.Object,
            mapper);


        var query = new GetOneDeviceQuery(
            deviceId.Value);


        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);


        // Assert
        result.Should().NotBeNull();

        result.DeviceId.Should().Be(deviceId.Value);
        result.DeviceName.Should().Be("Device-Test");

        repository.Verify(
            x => x.GetOneAsync(
                deviceId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}