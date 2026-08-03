using CollectManagement.Application.Features.Plannings.Commands.DeletePlanning;
using CollectManagement.Application.Interfaces.Repositories.Plannings;
using CollectManagement.Domain.Plannings;
using Moq;

namespace CollectManagement.Tests.Features.Plannings.Commands;

public class DeletePlanningCommandHandlerTests
{
    private readonly Mock<IPlanningRepository> _repository;
    private readonly DeletePlanningCommandHandler _handler;

    public DeletePlanningCommandHandlerTests()
    {
        _repository = new Mock<IPlanningRepository>();
        _handler = new DeletePlanningCommandHandler(_repository.Object);
    }

    [Fact]
    public async Task Handle_Should_Delete_Planning()
    {
        // Arrange
        var command = new DeletePlanningCommand(Ulid.NewUlid());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repository.Verify(
            x => x.DeleteAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Planning, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
