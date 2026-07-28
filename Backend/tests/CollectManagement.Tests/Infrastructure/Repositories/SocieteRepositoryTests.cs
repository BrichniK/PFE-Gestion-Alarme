using CollectManagement.Application.Interfaces.Societes;
using CollectManagementDomain.Societes;
using CollectManagementDomain.Societes.ValueObjects;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Infrastructure.Repositories;


public class SocieteRepositoryTests
{

    private readonly Mock<ISocieteRepository> _repository;


    public SocieteRepositoryTests()
    {
        _repository = new Mock<ISocieteRepository>();
    }



    [Fact]
    public async Task GetOneAsync_Should_Return_Societe()
    {

        var id =
            new SocieteId(Ulid.NewUlid());


        var societe =
            Societe.Create(
                id,
                null,
                "Societe Test",
                "MF001",
                "RNE001",
                10000,
                DateTime.UtcNow,
                "71111111",
                null,
                null,
                null,
                "test@test.com",
                "Adresse",
                "SOC001"
            );



        _repository
            .Setup(x => x.GetOneAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(societe);



        var result =
            await _repository.Object.GetOneAsync(
                id,
                CancellationToken.None);



        result.Should()
            .NotBeNull();


        result.Nom
            .Should()
            .Be("Societe Test");



        _repository.Verify(
            x => x.GetOneAsync(
                id,
                It.IsAny<CancellationToken>()),
            Times.Once);

    }

}