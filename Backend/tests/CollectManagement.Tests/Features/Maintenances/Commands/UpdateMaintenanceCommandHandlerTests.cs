using CollectManagement.Application.Features.Maintenances.Commands.UpdateMaintenance;
using CollectManagement.Application.Interfaces.Repositories.Maintenances;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Features.Maintenances.Commands;


public class UpdateMaintenanceCommandHandlerTests
{

    private readonly Mock<IMaintenanceRepository> _repository;
    private readonly UpdateMaintenanceCommandHandler _handler;


    public UpdateMaintenanceCommandHandlerTests()
    {
        _repository = new Mock<IMaintenanceRepository>();

        _handler = new UpdateMaintenanceCommandHandler(
            _repository.Object);
    }



    [Fact]
    public async Task HandleShouldUpdateMaintenance()
    {

        var command = new UpdateMaintenanceCommand(
            Ulid.NewUlid(),
            Ulid.NewUlid(),
            Ulid.NewUlid(),
            DateTime.UtcNow,
            null,
            null,
            null,
            null,
            null,
            "Update test"
        );


        await _handler.Handle(
            command,
            CancellationToken.None);



        _repository.Verify(
            x => x.UpdateBulkAsync(
                It.IsAny<CollectManagement.Domain.Maintenances.Maintenance>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}