using CollectManagement.Application.Features.Plannings.Commands.UpdatePlanning;
using CollectManagement.Application.Interfaces.Repositories.Plannings;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Employess.ObjectValues;
using CollectManagement.Domain.Groupes.ValueObjects;
using CollectManagement.Domain.Plannings;
using CollectManagement.Domain.Plannings.ValueObjects;
using CollectManagement.Domain.Shifts.ValueObjects;
using Moq;

namespace CollectManagement.Tests.Features.Plannings.Commands;

public class UpdatePlanningCommandHandlerTests
{
    private readonly Mock<IPlanningRepository> _repository;
    private readonly UpdatePlanningCommandHandler _handler;

    public UpdatePlanningCommandHandlerTests()
    {
        _repository = new Mock<IPlanningRepository>();
        _handler = new UpdatePlanningCommandHandler(_repository.Object);
    }

    [Fact]
    public async Task Handle_Should_Update_Planning_When_Exists()
    {
        // Arrange
        var planningId = new PlanningId(Ulid.NewUlid());

        var planning = Planning.Create(
            planningId,
            DateTime.UtcNow.Date,
            new[] { new GroupeId(Ulid.NewUlid()) },
            new[] { new DeviceId(Ulid.NewUlid()) },
            new[] { new ShiftId(Ulid.NewUlid()) },
            new[] { new EmployeeId(Ulid.NewUlid()) }
        );

        _repository
            .Setup(x => x.GetOneAsync(planningId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(planning);

        var command = new UpdatePlanningCommand
        {
            PlanningId = planningId.Value,
            Date = DateTime.UtcNow.Date.AddDays(1),
            GroupeId = Ulid.NewUlid(),
            DeviceId = Ulid.NewUlid(),
            ShiftId = Ulid.NewUlid(),
            EmployeeId = Ulid.NewUlid()
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repository.Verify(
            x => x.GetOneAsync(planningId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
