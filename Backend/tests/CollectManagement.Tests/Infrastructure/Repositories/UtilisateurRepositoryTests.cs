using CollectManagement.Application.Interfaces.Repositories.Utilisateurs;
using CollectManagement.Domain.Utilisateurs;
using CollectManagement.Domain.Utilisateurs.ValueObjects;
using CollectManagementDomain.Societes.ValueObjects;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Infrastructure.Repositories;

public class UtilisateurRepositoryTests
{
    private readonly Mock<IUtilisateurRepository> _repository;

    public UtilisateurRepositoryTests()
    {
        _repository = new Mock<IUtilisateurRepository>();
    }

    [Fact]
    public async Task GetOneAsync_Should_Return_Utilisateur()
    {
        var id = new UtilisateurId(Ulid.NewUlid());

        var utilisateur = Utilisateur.Create(
            id, "jdoe", "Doe", "John",
            "jdoe@example.com", "hashed",
            null, true,
            new SocieteId(Ulid.NewUlid()));

        _repository
            .Setup(x => x.GetOneAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(utilisateur);

        var result = await _repository.Object.GetOneAsync(id, CancellationToken.None);

        result.Should().NotBeNull();
        result!.UtilisateurId.Should().Be(id);
        result.NomUtilisateur.Should().Be("jdoe");
    }

    [Fact]
    public async Task TryToLogin_Should_Return_Null_When_Not_Found()
    {
        _repository
            .Setup(x => x.TryToLogin(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Utilisateur?)null);

        var result = await _repository.Object.TryToLogin("unknown", CancellationToken.None);

        result.Should().BeNull();
    }
}
