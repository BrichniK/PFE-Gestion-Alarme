using CollectManagement.Application.Features.JoursFeries.Queries.GetPagedListJourFerie;
using CollectManagement.Application.Interfaces.Repositories.JoursFeries;
using CollectManagement.Domain.JoursFeries;
using FluentAssertions;
using MapsterMapper;
using Moq;

namespace CollectManagement.Tests.Features.JoursFeries.Queries;

public class GetPagedListJourFerieQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Paged_JoursFeries()
    {
        // Arrange
        var repository = new Mock<IJourFerieRepository>();
        var mapper = new Mock<IMapper>();

        repository
            .Setup(x => x.GetPagedListAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<JourFerie>(), 0));

        mapper
            .Setup(x => x.Map<List<GetPagedListJourFerieDto>>(It.IsAny<List<JourFerie>>()))
            .Returns(new List<GetPagedListJourFerieDto>());

        var handler = new GetPagedListJourFerieQueryHandler(repository.Object, mapper.Object);
        var query = new GetPagedListJourFerieQuery(null, null, null, 1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        repository.Verify(
            x => x.GetPagedListAsync(null, null, null, 1, 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
