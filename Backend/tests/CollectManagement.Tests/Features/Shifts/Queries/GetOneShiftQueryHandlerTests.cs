using CollectManagement.Application.Features.Shifts.Queries.GetOneShift;
using CollectManagement.Application.Interfaces.Repositories.Shifts;
using CollectManagement.Domain.Shifts;
using CollectManagement.Domain.Shifts.ValueObjects;
using FluentAssertions;
using MapsterMapper;
using Moq;

namespace CollectManagement.Tests.Features.Shifts.Queries;

public class GetOneShiftQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Shift()
    {
        // Arrange
        var repository = new Mock<IShiftRepository>();
        var mapper = new Mock<IMapper>();

        var shiftId = new ShiftId(Ulid.NewUlid());

        var shift = Shift.Create(
            shiftId,
            "Matin",
            new TimeOnly(8, 0),
            new TimeOnly(16, 0)
        );

        var response = new GetOneShiftResponse(
            shiftId.Value,
            "Matin",
            new TimeOnly(8, 0),
            new TimeOnly(16, 0)
        );

        repository
            .Setup(x => x.GetOneAsync(shiftId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shift);

        mapper
            .Setup(x => x.Map<GetOneShiftResponse>(It.IsAny<Shift>()))
            .Returns(response);

        var handler = new GetOneShiftQueryHandler(repository.Object, mapper.Object);
        var query = new GetOneShiftQuery(shiftId.Value);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ShiftId.Should().Be(shiftId.Value);
        result.Label.Should().Be("Matin");

        repository.Verify(
            x => x.GetOneAsync(shiftId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
