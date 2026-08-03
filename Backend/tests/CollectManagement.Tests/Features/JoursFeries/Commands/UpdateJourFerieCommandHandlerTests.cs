using CollectManagement.Application.Features.JoursFeries.Commands.UpdateJourFerie;
using CollectManagement.Application.Interfaces.Repositories.JoursFeries;
using CollectManagement.Domain.JoursFeries;
using Moq;

namespace CollectManagement.Tests.Features.JoursFeries.Commands;

public class UpdateJourFerieCommandHandlerTests
{
    private readonly Mock<IJourFerieRepository> _repository;
    private readonly UpdateJourFerieCommandHandler _handler;

    public UpdateJourFerieCommandHandlerTests()
    {
        _repository = new Mock<IJourFerieRepository>();
        _handler = new UpdateJourFerieCommandHandler(_repository.Object);
    }

    [Fact]
    public async Task Handle_Should_Update_JourFerie()
    {
        // Arrange
        _repository
            .Setup(x => x.UpdateBulkAsync(It.IsAny<JourFerie>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new UpdateJourFerieCommand(
            Ulid.NewUlid(),
            new DateTime(2026, 5, 1),
            "Fête du Travail"
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repository.Verify(
            x => x.UpdateBulkAsync(It.IsAny<JourFerie>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
