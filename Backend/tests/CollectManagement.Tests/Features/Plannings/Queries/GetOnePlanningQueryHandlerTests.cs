using CollectManagement.Application.Features.Plannings.Queries.GetOnePlanning;
using CollectManagement.Application.Interfaces.Repositories.Plannings;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Employess.ObjectValues;
using CollectManagement.Domain.Groupes.ValueObjects;
using CollectManagement.Domain.Plannings;
using CollectManagement.Domain.Plannings.ValueObjects;
using CollectManagement.Domain.Shifts.ValueObjects;
using FluentAssertions;
using MapsterMapper;
using Moq;

namespace CollectManagement.Tests.Features.Plannings.Queries;

public class GetOnePlanningQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Planning()
    {
        // Arrange
        var repository = new Mock<IPlanningRepository>();
        var mapper = new Mock<IMapper>();

        var planningId = new PlanningId(Ulid.NewUlid());

        var planning = Planning.Create(
            planningId,
            DateTime.UtcNow.Date,
            new[] { new GroupeId(Ulid.NewUlid()) },
            new[] { new DeviceId(Ulid.NewUlid()) },
            new[] { new ShiftId(Ulid.NewUlid()) },
            Array.Empty<EmployeeId>()
        );

        var response = new GetOnePlanningResponse(
            planningId.Value,
            DateTime.UtcNow.Date,
            "group",
            new List<Ulid>(),
            new List<string>(),
            new List<Ulid>(),
            new List<Ulid>(),
            new List<Ulid>(),
            Ulid.Empty,
            null,
            Ulid.Empty,
            null,
            Ulid.Empty,
            null
        );

        repository
            .Setup(x => x.GetOneAsync(planningId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(planning);

        mapper
            .Setup(x => x.Map<GetOnePlanningResponse>(It.IsAny<Planning>()))
            .Returns(response);

        var handler = new GetOnePlanningQueryHandler(repository.Object, mapper.Object);
        var query = new GetOnePlanningQuery(planningId.Value);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PlanningId.Should().Be(planningId.Value);

        repository.Verify(
            x => x.GetOneAsync(planningId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
