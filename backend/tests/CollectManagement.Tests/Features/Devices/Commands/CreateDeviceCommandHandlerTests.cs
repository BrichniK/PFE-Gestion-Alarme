using CollectManagement.Application.Features.Devices.Commands.CreateDevice;
using CollectManagement.Application.Features.Devices.Mapping;
using CollectManagement.Application.Interfaces.Repositories.Devices;
using FluentAssertions;
using Mapster;
using MapsterMapper;
using Moq;

namespace CollectManagement.Tests.Features.Devices;

public class CreateDeviceCommandHandlerTests
{
    [Fact]
    public async Task HandleShouldCreateDeviceWhenCommandIsValid()
    {
       
        var repository = new Mock<IDeviceRepository>();

        var config = new TypeAdapterConfig();
        new DeviceMapping().Register(config);

        var mapper = new Mapper(config);

        var handler = new CreateDeviceCommandHandler(
            repository.Object,
            mapper);

        var deviceName = "Device-Test";
        var matricule = "MAT-001";
        var nombreCapteur = 4;

        var command = new CreateDeviceCommand(
            deviceName,
            matricule,
            nombreCapteur
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        repository.Verify(
            x => x.AddAsync(
                It.IsAny<CollectManagement.Domain.Devices.Device>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        result.Should().NotBeNull();
        result.DeviceId.Should().NotBe(Ulid.Empty);
    }
}