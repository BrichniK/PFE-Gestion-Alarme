using CollectManagement.Application.Features.Maintenances.Commands.DeleteMaintenance;
using CollectManagement.Application.Interfaces.Repositories.Maintenances;
using CollectManagement.Domain.Maintenances;
using Moq;

namespace CollectManagement.Tests.Features.Maintenances.Commands;


public class DeleteMaintenanceCommandHandlerTests
{

    [Fact]
    public async Task HandleShouldDeleteMaintenance()
    {

        var repository = new Mock<IMaintenanceRepository>();


        var handler = new DeleteMaintenanceCommandHandler(
            repository.Object);



        var command = new DeleteMaintenanceCommand(
            Ulid.NewUlid()
        );



        await handler.Handle(
            command,
            CancellationToken.None);



        repository.Verify(
            x => x.DeleteAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Maintenance,bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

    }
}