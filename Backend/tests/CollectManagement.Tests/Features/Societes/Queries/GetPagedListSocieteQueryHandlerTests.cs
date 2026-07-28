using Moq;
using FluentAssertions;
using CollectManagement.Application.Interfaces.Societes;
using CollectManagement.Application.Features.Societes.Queries.GetPagedListSociete;
using CollectManagementDomain.Societes;


namespace CollectManagement.Tests.Features.Societes.Queries;


public class GetPagedListSocieteQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_List()
    {
        var repo = new Mock<ISocieteRepository>();
        var mapper = new Mock<MapsterMapper.IMapper>();


        repo.Setup(x => x.GetPagedListAsync(
                null,
                null,
                null,
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (new List<Societe>(), 0)
            );


        mapper.Setup(x => x.Map<List<GetPagedListSocieteDto>>(
                It.IsAny<object>()))
            .Returns(new List<GetPagedListSocieteDto>());


        var handler =
            new GetPagedListSocieteQueryHandler(
                repo.Object,
                mapper.Object
            );


        var result =
            await handler.Handle(
                new GetPagedListSocieteQuery(
                    null,
                    null,
                    null,
                    1,
                    10
                ),
                CancellationToken.None
            );


        result.Should().NotBeNull();
    }
}