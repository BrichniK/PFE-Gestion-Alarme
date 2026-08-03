using CollectManagement.Application.Interfaces.Repositories.Shifts;
using CollectManagement.Domain.Shifts;
using CollectManagement.Domain.Shifts.ValueObjects;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Infrastructure.Repositories;

public class ShiftRepositoryTests
{
    private readonly Mock<IShiftRepository> _repository;

    public ShiftRepositoryTests()
    {
        _repository = new Mock<IShiftRepository>();
    }

    [Fact]
    public async Task GetOneAsync_Should_Return_Shift()
    {
        // Arrange
        var id = new ShiftId(Ulid.NewUlid());

        var shift = Shift.Create(id, "Matin", new TimeOnly(8, 0), new TimeOnly(16, 0));

        _repository
            .Setup(x => x.GetOneAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shift);

        // Act
        var result = await _repository.Object.GetOneAsync(id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ShiftId.Should().Be(id);
        result.Label.Should().Be("Matin");

        _repository.Verify(
            x => x.GetOneAsync(id, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
