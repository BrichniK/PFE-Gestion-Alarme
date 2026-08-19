using CollectManagement.Application.Contracts.Authentication;
using CollectManagement.Application.Exceptions;
using CollectManagement.Application.Features.Utilisateurs.Queries.Login;
using CollectManagement.Application.Interfaces.Authentification;
using CollectManagement.Application.Interfaces.Repositories.Utilisateurs;
using CollectManagement.Application.Interfaces.Services;
using CollectManagement.Domain.Utilisateurs;
using CollectManagement.Domain.Utilisateurs.ValueObjects;
using CollectManagementDomain.Societes.ValueObjects;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Features.Utilisateurs.Queries;

public class LoginQueryHandlerTests
{
    private readonly Mock<IUtilisateurRepository> _repository;
    private readonly Mock<IPasswordService>       _passwordService;
    private readonly Mock<IJwtTokenGenerator>     _tokenGenerator;
    private readonly LoginQueryHandler            _handler;

    public LoginQueryHandlerTests()
    {
        _repository      = new Mock<IUtilisateurRepository>();
        _passwordService = new Mock<IPasswordService>();
        _tokenGenerator  = new Mock<IJwtTokenGenerator>();
        _handler         = new LoginQueryHandler(
            _repository.Object,
            _passwordService.Object,
            _tokenGenerator.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Token_When_Credentials_Valid()
    {
        var utilisateurId = new UtilisateurId(Ulid.NewUlid());
        var utilisateur   = Utilisateur.Create(
            utilisateurId, "jdoe", "Doe", "John",
            "jdoe@example.com", "hashed_pw",
            null, true,
            new SocieteId(Ulid.NewUlid()));

        _repository
            .Setup(x => x.TryToLogin("jdoe", It.IsAny<CancellationToken>()))
            .ReturnsAsync(utilisateur);

        _passwordService
            .Setup(x => x.HashPassword(utilisateurId, "secret"))
            .Returns("hashed_pw");

        _tokenGenerator
            .Setup(x => x.GenerateToken(utilisateur))
            .Returns("jwt_token");

        var query  = new LoginQuery("jdoe", "secret");
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Token.Should().Be("jwt_token");
        result.NomUtilisateur.Should().Be("jdoe");
    }

    [Fact]
    public async Task Handle_Should_Throw_When_User_Not_Found()
    {
        _repository
            .Setup(x => x.TryToLogin(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Utilisateur?)null);

        var act = async () => await _handler.Handle(
            new LoginQuery("unknown", "pass"),
            CancellationToken.None);

        await act.Should().ThrowAsync<BadCredentialException>();
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Password_Wrong()
    {
        var utilisateurId = new UtilisateurId(Ulid.NewUlid());
        var utilisateur   = Utilisateur.Create(
            utilisateurId, "jdoe", "Doe", "John",
            "jdoe@example.com", "correct_hash",
            null, true,
            new SocieteId(Ulid.NewUlid()));

        _repository
            .Setup(x => x.TryToLogin("jdoe", It.IsAny<CancellationToken>()))
            .ReturnsAsync(utilisateur);

        _passwordService
            .Setup(x => x.HashPassword(utilisateurId, "wrong"))
            .Returns("wrong_hash");

        var act = async () => await _handler.Handle(
            new LoginQuery("jdoe", "wrong"),
            CancellationToken.None);

        await act.Should().ThrowAsync<BadCredentialException>();
    }
}
