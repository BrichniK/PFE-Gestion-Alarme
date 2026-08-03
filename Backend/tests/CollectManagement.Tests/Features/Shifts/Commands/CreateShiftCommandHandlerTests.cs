using CollectManagement.Application.Features.Shifts.Commands.CreateShift;
using CollectManagement.Application.Interfaces.Repositories.Shifts;
using CollectManagement.Domain.Shifts;
using FluentAssertions;
using MapsterMapper;
using Moq;

namespace CollectManagement.Tests.Features.Shifts.Commands;

public class CreateShiftCommandHandlerTests
{
    private readonly Mock<IShiftRepository> _repository;
    private readonly Mock<IMapper> _mapper;
    private readonly CreateShiftCommandHandler _handler;

    public CreateShiftCommandHandlerTests()
    {
        _repository = new Mock<IShiftRepository>();
        _mapper = new Mock<IMapper>();
        _handler = new CreateShiftCommandHandler(_repository.Object, _mapper.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_Shift()
    {
        // Arrange
        var response = new CreateShiftResponse(Ulid.NewUlid());

        _mapper
            .Setup(x => x.Map<CreateShiftResponse>(It.IsAny<Shift>()))
            .Returns(response);

        var command = new CreateShiftCommand(
            "Matin",
            new TimeOnly(8, 0),
            new TimeOnly(16, 0)
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ShiftId.Should().NotBe(Ulid.Empty);

        _repository.Verify(
            x => x.AddAsync(It.IsAny<Shift>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
