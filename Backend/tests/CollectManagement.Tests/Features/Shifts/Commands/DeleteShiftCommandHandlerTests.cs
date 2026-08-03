using CollectManagement.Application.Features.Shifts.Commands.DeleteShift;
using CollectManagement.Application.Interfaces.Repositories.Shifts;
using CollectManagement.Domain.Shifts;
using Moq;

namespace CollectManagement.Tests.Features.Shifts.Commands;

public class DeleteShiftCommandHandlerTests
{
    private readonly Mock<IShiftRepository> _repository;
    private readonly DeleteShiftCommandHandler _handler;

    public DeleteShiftCommandHandlerTests()
    {
        _repository = new Mock<IShiftRepository>();
        _handler = new DeleteShiftCommandHandler(_repository.Object);
    }

    [Fact]
    public async Task Handle_Should_Delete_Shift()
    {
        // Arrange
        var command = new DeleteShiftCommand(Ulid.NewUlid());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repository.Verify(
            x => x.DeleteAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Shift, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
