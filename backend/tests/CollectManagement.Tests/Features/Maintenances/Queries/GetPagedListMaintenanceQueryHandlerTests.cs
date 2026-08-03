using CollectManagement.Application.Features.Maintenances.Queries.GetPagedListMaintenance;
using CollectManagement.Application.Interfaces.Repositories.Maintenances;
using FluentAssertions;
using MapsterMapper;
using Moq;

namespace CollectManagement.Tests.Features.Maintenances.Queries;

public class GetPagedListMaintenanceQueryHandlerTests
{
    [Fact]
    public async Task HandleShouldReturnPagedMaintenances()
    {
        var repository = new Mock<IMaintenanceRepository>();
        var mapper = new Mock<IMapper>();

        repository
            .Setup(x => x.GetPagedListAsync(
                null,
                null,
                null,
                1,
                10,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (
                    new List<CollectManagement.Domain.Maintenances.Maintenance>(),
                    0
                ));


        var handler = new GetPagedListMaintenanceQueryHandler(
            repository.Object,
            mapper.Object);


        var query = new GetPagedListMaintenanceQuery(
            null,
            null,
            null,
            1,
            10,
            null,
            null,
            null);


        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);


        // Assert
        result.Should().NotBeNull();

        repository.Verify(
            x => x.GetPagedListAsync(
                null,
                null,
                null,
                1,
                10,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}