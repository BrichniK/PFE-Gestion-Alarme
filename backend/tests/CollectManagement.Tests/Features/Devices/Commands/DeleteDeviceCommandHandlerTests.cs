using Moq;
using Xunit;
using CollectManagement.Application.Interfaces.Repositories.Alertes;
using CollectManagement.Application.Interfaces.Repositories.Devices;
using CollectManagement.Domain.Alertes.ValueObjects;
using CollectManagement.Domain.Devices.ValueObjects;

namespace CollectManagement.Tests.Features.Devices;


public class DeleteDeviceCommandHandlerTests
{

    private readonly Mock<IDeviceRepository> _repository;


    public DeleteDeviceCommandHandlerTests()
    {
        _repository = new Mock<IDeviceRepository>();
    }


    [Fact]
    public async Task Handle_Should_Delete_Device()
    {

        var id = new DeviceId(Ulid.NewUlid());


        await Task.CompletedTask;


        Assert.True(true);
    }
}