using CollectManagement.Application.Features.JoursFeries.Commands.DeleteJourFerie;
using CollectManagement.Application.Interfaces.Repositories.JoursFeries;
using CollectManagement.Domain.JoursFeries;
using Moq;

namespace CollectManagement.Tests.Features.JoursFeries.Commands;

public class DeleteJourFerieCommandHandlerTests
{
    private readonly Mock<IJourFerieRepository> _repository;
    private readonly DeleteJourFerieCommandHandler _handler;

    public DeleteJourFerieCommandHandlerTests()
    {
        _repository = new Mock<IJourFerieRepository>();
        _handler = new DeleteJourFerieCommandHandler(_repository.Object);
    }

    [Fact]
    public async Task Handle_Should_Delete_JourFerie()
    {
        // Arrange
        var command = new DeleteJourFerieCommand(Ulid.NewUlid());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repository.Verify(
            x => x.DeleteAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<JourFerie, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
