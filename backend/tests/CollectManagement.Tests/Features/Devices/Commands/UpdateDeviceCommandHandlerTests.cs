using CollectManagement.Application.Features.Devices.Commands.UpdateDevice;
using CollectManagement.Application.Interfaces.Repositories.Devices;
using CollectManagement.Domain.Devices;
using CollectManagement.Domain.Devices.ValueObjects;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Features.Devices;

public class UpdateDeviceCommandHandlerTests
{
    private readonly Mock<IDeviceRepository> _repository;
    private readonly UpdateDeviceCommandHandler _handler;


    public UpdateDeviceCommandHandlerTests()
    {
        _repository = new Mock<IDeviceRepository>();

        _handler = new UpdateDeviceCommandHandler(
            _repository.Object
        );
    }


    [Fact]
    public async Task HandleShouldUpdateDeviceWhenExists()
    {
        // Arrange
        var id = new DeviceId(Ulid.NewUlid());


        var device = Device.Create(
            id,
            "Device-Test",
            "MAT-001",
            4
        );


        _repository
            .Setup(x => x.GetOneAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);



        var command = new UpdateDeviceCommand(
            id.Value,
            "Device-Updated",
            "MAT-002",
            6
        );


        // Act
        await _handler.Handle(
            command,
            CancellationToken.None);



        // Assert
        _repository.Verify(
            x => x.UpdateBulkAsync(
                It.IsAny<Device>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}