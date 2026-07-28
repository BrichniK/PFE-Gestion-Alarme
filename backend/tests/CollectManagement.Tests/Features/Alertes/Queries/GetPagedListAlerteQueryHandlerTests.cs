using CollectManagement.Application.Features.Alertes.Queries.GetPagedListAlerte;
using CollectManagement.Application.Interfaces.Repositories.Alertes;
using FluentAssertions;
using MapsterMapper;
using Moq;

namespace CollectManagement.Tests.Features.Alertes;

public class GetPagedListAlerteQueryHandlerTests
{
    [Fact]
    public async Task HandleShouldReturnPagedResult()
    {
        var repository = new Mock<IAlerteRepository>();
        var mapper = new Mock<IMapper>();

        repository
            .Setup(x => x.GetPagedListAsync(
                null,
                null,
                null,
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (
                    new List<CollectManagement.Domain.Alertes.Alerte>(),
                    0
                ));


        var handler =
            new GetPagedListAlerteQueryHandler(
                repository.Object,
                mapper.Object);


        var query = new GetPagedListAlerteQuery(
            null,
            null,
            null,
            1,
            10);


        var result = await handler.Handle(
            query,
            CancellationToken.None);


        result.Should().NotBeNull();


        repository.Verify(
            x => x.GetPagedListAsync(
                null,
                null,
                null,
                1,
                10,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}