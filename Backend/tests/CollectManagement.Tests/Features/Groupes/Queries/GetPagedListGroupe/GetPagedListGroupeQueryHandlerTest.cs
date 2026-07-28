using CollectManagement.Application.Features.Groupes.Queries.GetPagedListGroupe;
using CollectManagement.Application.Interfaces.Groupes;
using CollectManagement.Domain.Groupes;
using FluentAssertions;
using MapsterMapper;
using Moq;

namespace CollectManagement.Tests.Features.Groupes.Queries;


public class GetPagedListGroupeQueryHandlerTests
{

    [Fact]
    public async Task Handle_Should_Return_Paged_Groupes()
    {

        var repository =
            new Mock<IGroupeRepository>();

        var mapper =
            new Mock<IMapper>();



        var groupes =
            new List<Groupe>();


        repository
            .Setup(x => x.GetPagedListAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (
                    groupes,
                    0
                ));



        mapper
            .Setup(x => x.Map<List<GetPagedListGroupeDto>>(
                It.IsAny<List<Groupe>>()))
            .Returns(new List<GetPagedListGroupeDto>());



        var handler =
            new GetPagedListGroupeQueryHandler(
                repository.Object,
                mapper.Object);



        var query =
            new GetPagedListGroupeQuery(
                null,
                null,
                null,
                1,
                10);



        var result =
            await handler.Handle(
                query,
                CancellationToken.None);



        result.Should()
            .NotBeNull();

    }
}