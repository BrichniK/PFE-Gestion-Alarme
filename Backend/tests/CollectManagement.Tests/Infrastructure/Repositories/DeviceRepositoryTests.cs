using CollectManagement.Application.Interfaces.Repositories.Devices;
using CollectManagement.Domain.Devices;
using CollectManagement.Domain.Devices.ValueObjects;
using FluentAssertions;
using Moq;


namespace CollectManagement.Tests.Infrastructure.Repositories;


public class DeviceRepositoryTests
{

    private readonly Mock<IDeviceRepository> _repository;


    public DeviceRepositoryTests()
    {
        _repository = new Mock<IDeviceRepository>();
    }



    [Fact]
    public async Task GetOneAsync_Should_Return_Device()
    {

        var id =
            new DeviceId(Ulid.NewUlid());



        var device =
            Device.Create(
                id,
                "Machine001",
                "CAPTEUR001",
                1
            );



        _repository
            .Setup(x=>x.GetOneAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);



        var result =
            await _repository.Object.GetOneAsync(
                id,
                CancellationToken.None);



        result.Should()
            .NotBeNull();


        result.DeviceId
            .Should()
            .Be(id);

    }

}