using CollectManagement.Application.Features.Shifts.Commands.UpdateShift;
using CollectManagement.Application.Interfaces.Repositories.Shifts;
using CollectManagement.Domain.Shifts;
using Moq;

namespace CollectManagement.Tests.Features.Shifts.Commands;

public class UpdateShiftCommandHandlerTests
{
    private readonly Mock<IShiftRepository> _repository;
    private readonly UpdateShiftCommandHandler _handler;

    public UpdateShiftCommandHandlerTests()
    {
        _repository = new Mock<IShiftRepository>();
        _handler = new UpdateShiftCommandHandler(_repository.Object);
    }

    [Fact]
    public async Task Handle_Should_Update_Shift()
    {
        // Arrange
        _repository
            .Setup(x => x.UpdateBulkAsync(It.IsAny<Shift>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new UpdateShiftCommand(
            Ulid.NewUlid(),
            "Soir",
            new TimeOnly(16, 0),
            new TimeOnly(23, 0)
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repository.Verify(
            x => x.UpdateBulkAsync(It.IsAny<Shift>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
