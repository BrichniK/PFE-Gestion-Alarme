using CollectManagement.Application.Features.Maintenances.Mapping;
using CollectManagement.Application.Features.Maintenances.Queries.GetOneMaintenance;
using CollectManagement.Application.Interfaces.Repositories.Maintenances;
using CollectManagement.Domain.Maintenances;
using CollectManagement.Domain.Maintenances.ObjectValues;
using FluentAssertions;
using Mapster;
using MapsterMapper;
using Moq;


namespace CollectManagement.Tests.Features.Maintenances.Queries;


public class GetOneMaintenanceQueryHandlerTests
{

    [Fact]
    public async Task Handle_Should_Return_Maintenance_When_Exists()
    {

        var repository = new Mock<IMaintenanceRepository>();


        var config = new TypeAdapterConfig();
        config.Scan(typeof(MaintenanceMapping).Assembly);


        var mapper = new Mapper(config);



        var id = new MaintenanceId(
            Ulid.NewUlid()
        );



        var maintenance = Maintenance.Create(
            id,
            new(Ulid.NewUlid()),
            new(Ulid.NewUlid()),
            DateTime.UtcNow,
            null,
            null,
            null,
            null,
            null,
            "test"
        );



        repository
            .Setup(x => x.GetOneAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(maintenance);



        var handler = new GetOneMaintenanceQueryHandler(
            repository.Object,
            mapper);



        var query = new GetOneMaintenanceQuery(
            id.Value);



        var result = await handler.Handle(
            query,
            CancellationToken.None);



        result.Should().NotBeNull();


        repository.Verify(
            x => x.GetOneAsync(
                id,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}