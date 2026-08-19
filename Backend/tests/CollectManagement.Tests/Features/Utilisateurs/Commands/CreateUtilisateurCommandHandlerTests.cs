using CollectManagement.Application.Features.Utilisateurs.Commands.CreateUtilisateur;
using CollectManagement.Application.Interfaces.Repositories.Utilisateurs;
using CollectManagement.Application.Interfaces.Services;
using CollectManagement.Application.Interfaces.Societes;
using CollectManagement.Domain.Utilisateurs;
using CollectManagement.Domain.Utilisateurs.ValueObjects;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Features.Utilisateurs.Commands;

public class CreateUtilisateurCommandHandlerTests
{
    private readonly Mock<IUtilisateurRepository> _repository;
    private readonly Mock<IPasswordService> _passwordService;
    private readonly Mock<ISocieteRepository> _societeRepository;

    public CreateUtilisateurCommandHandlerTests()
    {
        _repository        = new Mock<IUtilisateurRepository>();
        _passwordService   = new Mock<IPasswordService>();
        _societeRepository = new Mock<ISocieteRepository>();

        _passwordService
            .Setup(x => x.HashPassword(
                It.IsAny<UtilisateurId>(),
                It.IsAny<string>()))
            .Returns("hashed_pw");
    }

    [Fact]
    public async Task Handle_Should_Create_Utilisateur()
    {
        var handler = new CreateUtilisateurCommandHandler(
            _repository.Object,
            _passwordService.Object,
            _societeRepository.Object);

        var command = new CreateUtilisateurCommand(
            "jdoe",
            "Doe",
            "John",
            "jdoe@example.com",
            "password123",
            null,
            true,
            Ulid.NewUlid());

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.UtilisateurId.Should().NotBe(Ulid.Empty);

        _repository.Verify(
            x => x.AddAsync(
                It.IsAny<Utilisateur>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _passwordService.Verify(
            x => x.HashPassword(
                It.IsAny<UtilisateurId>(),
                "password123"),
            Times.Once);
    }
}
