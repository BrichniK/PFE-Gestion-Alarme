using CollectManagement.Application.Interfaces.Repositories.Maintenances;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Employess.ObjectValues;
using CollectManagement.Domain.Maintenances;
using CollectManagement.Domain.Maintenances.ObjectValues;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Infrastructure.Repositories;

public class MaintenanceRepositoryTests
{

    private readonly Mock<IMaintenanceRepository> _repository;


    public MaintenanceRepositoryTests()
    {
        _repository = new Mock<IMaintenanceRepository>();
    }


    [Fact]
    public async Task GetOneAsync_Should_Return_Maintenance()
    {

        var id       = new MaintenanceId(Ulid.NewUlid());
        var deviceId = new DeviceId(Ulid.NewUlid());
        var empId    = new EmployeeId(Ulid.NewUlid());

        var maintenance = Maintenance.Create(
            id, deviceId, empId,
            null, null, null, null, null, null,
            "Inspection moteur"
        );


        _repository
            .Setup(x => x.GetOneAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(maintenance);


        var result = await _repository.Object.GetOneAsync(id, CancellationToken.None);


        result.Should().NotBeNull();

        result.MaintenanceId.Should().Be(id);

        result.Description.Should().Be("Inspection moteur");
    }


    [Fact]
    public async Task GetLatestByDeviceIdAsync_Should_Return_Maintenance()
    {

        var deviceId = new DeviceId(Ulid.NewUlid());

        var maintenance = Maintenance.Create(
            new MaintenanceId(Ulid.NewUlid()),
            deviceId,
            new EmployeeId(Ulid.NewUlid()),
            null, null, null, null, null, null,
            "Dernière maintenance"
        );


        _repository
            .Setup(x => x.GetLatestByDeviceIdAsync(deviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(maintenance);


        var result = await _repository.Object.GetLatestByDeviceIdAsync(deviceId, CancellationToken.None);


        result.Should().NotBeNull();

        result!.DeviceId.Should().Be(deviceId);
    }
}
