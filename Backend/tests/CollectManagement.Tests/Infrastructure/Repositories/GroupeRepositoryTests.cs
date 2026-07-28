using CollectManagement.Application.Interfaces.Groupes;
using CollectManagement.Domain.Groupes;
using CollectManagement.Domain.Groupes.ValueObjects;
using FluentAssertions;
using Moq;


namespace CollectManagement.Tests.Infrastructure.Repositories;


public class GroupeRepositoryTests
{

    private readonly Mock<IGroupeRepository> _repository;


    public GroupeRepositoryTests()
    {
        _repository = new Mock<IGroupeRepository>();
    }



    [Fact]
    public async Task GetOneAsync_Should_Return_Groupe()
    {

        var id =
            new GroupeId(Ulid.NewUlid());



        var groupe =
            Groupe.Create(
                id,
                "Equipe Maintenance",
                "#FFFFFF",
                new List<Ulid>()
            );



        _repository
            .Setup(x => x.GetOneAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(groupe);



        var result =
            await _repository.Object.GetOneAsync(
                id,
                CancellationToken.None);



        result.Should()
            .NotBeNull();


        result.Nom
            .Should()
            .Be("Equipe Maintenance");



        _repository.Verify(
            x => x.GetOneAsync(
                id,
                It.IsAny<CancellationToken>()),
            Times.Once);

    }

}