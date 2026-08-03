using CollectManagement.Application.Interfaces.Repositories.Plannings;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Employess.ObjectValues;
using CollectManagement.Domain.Groupes.ValueObjects;
using CollectManagement.Domain.Plannings;
using CollectManagement.Domain.Plannings.ValueObjects;
using CollectManagement.Domain.Shifts.ValueObjects;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Infrastructure.Repositories;

public class PlanningRepositoryTests
{
    private readonly Mock<IPlanningRepository> _repository;

    public PlanningRepositoryTests()
    {
        _repository = new Mock<IPlanningRepository>();
    }

    [Fact]
    public async Task GetOneAsync_Should_Return_Planning()
    {
        // Arrange
        var id = new PlanningId(Ulid.NewUlid());

        var planning = Planning.Create(
            id,
            DateTime.UtcNow.Date,
            new[] { new GroupeId(Ulid.NewUlid()) },
            new[] { new DeviceId(Ulid.NewUlid()) },
            new[] { new ShiftId(Ulid.NewUlid()) },
            Array.Empty<EmployeeId>()
        );

        _repository
            .Setup(x => x.GetOneAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(planning);

        // Act
        var result = await _repository.Object.GetOneAsync(id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PlanningId.Should().Be(id);

        _repository.Verify(
            x => x.GetOneAsync(id, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
