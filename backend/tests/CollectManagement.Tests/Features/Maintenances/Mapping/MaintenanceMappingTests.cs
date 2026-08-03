using CollectManagement.Application.Features.Maintenances.Commands.CreateMaintenance;
using CollectManagement.Application.Features.Maintenances.Mapping;
using CollectManagement.Application.Features.Maintenances.Queries.GetOneMaintenance;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Employess.ObjectValues;
using CollectManagement.Domain.Maintenances;
using CollectManagement.Domain.Maintenances.ObjectValues;
using FluentAssertions;
using Mapster;

namespace CollectManagement.Tests.Features.Maintenances.Mapping;

public class MaintenanceMappingTests
{
    private readonly TypeAdapterConfig _config;


    public MaintenanceMappingTests()
    {
        _config = new TypeAdapterConfig();

        _config.Scan(typeof(MaintenanceMapping).Assembly);
    }


    [Fact]
    public void ShouldMapMaintenanceToCreateMaintenanceResponse()
    {
        // Arrange
        var maintenanceId = new MaintenanceId(Ulid.NewUlid());

        var maintenance =
            CollectManagement.Domain.Maintenances.Maintenance.Create(
                maintenanceId,
                new DeviceId(Ulid.NewUlid()),
                new EmployeeId(Ulid.NewUlid()),
                DateTime.UtcNow,
                null,
                null,
                null,
                null,
                null,
                "Maintenance test"
            );


        // Act
        var result = maintenance.Adapt<CreateMaintenanceResponse>(_config);


        // Assert
        result.Should().NotBeNull();

        result.MaintenanceId
            .Should()
            .Be(maintenanceId.Value);
    }



    [Fact]
    public void ShouldMapMaintenanceToGetOneMaintenanceResponse()
    {
        // Arrange
        var maintenanceId = new MaintenanceId(Ulid.NewUlid());

        var maintenance =
            CollectManagement.Domain.Maintenances.Maintenance.Create(
                maintenanceId,
                new DeviceId(Ulid.NewUlid()),
                new EmployeeId(Ulid.NewUlid()),
                DateTime.UtcNow,
                null,
                null,
                null,
                null,
                null,
                "Maintenance test"
            );


        // Act
        var result = maintenance.Adapt<GetOneMaintenanceResponse>(_config);


        // Assert
        result.Should().NotBeNull();

        result.MaintenanceId
            .Should()
            .Be(maintenanceId.Value);

        result.Description
            .Should()
            .Be("Maintenance test");
    }
}