using CollectManagement.Application.Exceptions;
using CollectManagement.Application.Features.Utilisateurs.Queries.LoginCheck;
using CollectManagement.Application.Interfaces.Authentification;
using CollectManagement.Application.Interfaces.Repositories.Utilisateurs;
using CollectManagement.Domain.Utilisateurs;
using CollectManagement.Domain.Utilisateurs.ValueObjects;
using CollectManagementDomain.Societes.ValueObjects;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Features.Utilisateurs.Queries;

public class LoginCheckQueryHandlerTests
{
    private readonly Mock<IUtilisateurRepository> _repository;
    private readonly Mock<IJwtTokenGenerator>     _tokenGenerator;
    private readonly LoginCheckQueryHandler       _handler;

    public LoginCheckQueryHandlerTests()
    {
        _repository     = new Mock<IUtilisateurRepository>();
        _tokenGenerator = new Mock<IJwtTokenGenerator>();
        _handler        = new LoginCheckQueryHandler(_repository.Object, _tokenGenerator.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Token_When_User_Found()
    {
        var utilisateurId = new UtilisateurId(Ulid.NewUlid());
        var utilisateur   = Utilisateur.Create(
            utilisateurId, "jdoe", "Doe", "John",
            "jdoe@example.com", "hashed",
            null, true,
            new SocieteId(Ulid.NewUlid()));

        _repository
            .Setup(x => x.GetOneAsync(utilisateurId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(utilisateur);

        _tokenGenerator
            .Setup(x => x.GenerateToken(utilisateur))
            .Returns("jwt_check_token");

        var query  = new LoginCheckQuery(utilisateurId.Value);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Token.Should().Be("jwt_check_token");
        result.UtilisateurId.Should().Be(utilisateurId.Value);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_User_Not_Found()
    {
        _repository
            .Setup(x => x.GetOneAsync(
                It.IsAny<UtilisateurId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Utilisateur?)null);

        var act = async () => await _handler.Handle(
            new LoginCheckQuery(Ulid.NewUlid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<BadCredentialException>();
    }
}
