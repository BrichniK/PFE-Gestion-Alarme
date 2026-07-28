using CollectManagement.Application.Features.Groupes.Queries.GetOneGroupe;
using CollectManagement.Application.Interfaces.Groupes;
using CollectManagement.Domain.Groupes;
using CollectManagement.Domain.Groupes.ValueObjects;
using FluentAssertions;
using MapsterMapper;
using Moq;

namespace CollectManagement.Tests.Features.Groupes.Queries;

public class GetOneGroupeQueryHandlerTests
{

    [Fact]
    public async Task Handle_Should_Return_Groupe()
    {

        // Arrange

        var repository =
            new Mock<IGroupeRepository>();

        var mapper =
            new Mock<IMapper>();


        var groupeId =
            new GroupeId(Ulid.NewUlid());


        var groupe =
            Groupe.Create(
                groupeId,
                "Equipe",
                "#FFF",
                new List<Ulid>());



        var response =
            new GetOneGroupeResponse(
                groupeId.Value,
                "Equipe",
                "#FFF",
                new List<Ulid>()
            );



        repository
            .Setup(x => x.GetOneAsync(
                It.IsAny<GroupeId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(groupe);



        mapper
            .Setup(x => x.Map<GetOneGroupeResponse>(
                It.IsAny<Groupe>()))
            .Returns(response);



        var handler =
            new GetOneGroupeQueryHandler(
                repository.Object,
                mapper.Object);



        var query =
            new GetOneGroupeQuery(
                groupeId.Value);



        // Act

        var result =
            await handler.Handle(
                query,
                CancellationToken.None);



        // Assert

        result.Should()
            .NotBeNull();


        result.Nom
            .Should()
            .Be("Equipe");


        mapper.Verify(
            x => x.Map<GetOneGroupeResponse>(
                It.IsAny<Groupe>()),
            Times.Once);

    }
}