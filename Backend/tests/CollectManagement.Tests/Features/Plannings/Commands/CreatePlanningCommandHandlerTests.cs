using CollectManagement.Application.Features.Plannings.Commands.CreatePlanning;
using CollectManagement.Application.Interfaces.Repositories.Plannings;
using CollectManagement.Domain.Plannings;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Features.Plannings.Commands;

public class CreatePlanningCommandHandlerTests
{
    private readonly Mock<IPlanningRepository> _repository;
    private readonly CreatePlanningCommandHandler _handler;

    public CreatePlanningCommandHandlerTests()
    {
        _repository = new Mock<IPlanningRepository>();
        _handler = new CreatePlanningCommandHandler(_repository.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_Single_Planning()
    {
        // Arrange
        var command = new CreatePlanningCommand
        {
            Date = DateTime.UtcNow.Date,
            GroupeId = Ulid.NewUlid(),
            DeviceId = Ulid.NewUlid(),
            ShiftId = Ulid.NewUlid(),
            EmployeeId = Ulid.NewUlid()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PlanningId.Should().NotBe(Ulid.Empty);
        result.PlanningIds.Should().HaveCount(1);

        _repository.Verify(
            x => x.AddAsync(It.IsAny<Planning>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Create_Multiple_Plannings_When_Multiple_Dates()
    {
        // Arrange
        var command = new CreatePlanningCommand
        {
            Dates = new List<DateTime>
            {
                DateTime.UtcNow.Date,
                DateTime.UtcNow.Date.AddDays(1)
            },
            GroupeId = Ulid.NewUlid(),
            DeviceId = Ulid.NewUlid(),
            ShiftId = Ulid.NewUlid(),
            EmployeeId = Ulid.NewUlid()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PlanningIds.Should().HaveCount(2);

        _repository.Verify(
            x => x.AddRangeAsync(It.IsAny<List<Planning>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
