using CollectManagement.Application.Features.Plannings.Queries.GetPagedListPlanning;
using CollectManagement.Application.Interfaces.Repositories.Plannings;
using CollectManagement.Domain.Plannings;
using FluentAssertions;
using MapsterMapper;
using Moq;

namespace CollectManagement.Tests.Features.Plannings.Queries;

public class GetPagedListPlanningQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Paged_Plannings()
    {
        // Arrange
        var repository = new Mock<IPlanningRepository>();
        var mapper = new Mock<IMapper>();

        repository
            .Setup(x => x.GetPagedListAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Planning>(), 0));

        mapper
            .Setup(x => x.Map<List<GetPagedListPlanningDto>>(It.IsAny<List<Planning>>()))
            .Returns(new List<GetPagedListPlanningDto>());

        var handler = new GetPagedListPlanningQueryHandler(repository.Object, mapper.Object);
        var query = new GetPagedListPlanningQuery(null, null, null, 1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        repository.Verify(
            x => x.GetPagedListAsync(null, null, null, 1, 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
