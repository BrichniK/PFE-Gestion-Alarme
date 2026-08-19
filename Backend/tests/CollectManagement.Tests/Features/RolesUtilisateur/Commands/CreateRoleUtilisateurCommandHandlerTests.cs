using CollectManagement.Application.Features.RolesUtilisateur.Commands.CreateRoleUtilisateur;
using CollectManagement.Application.Interfaces.Repositories.Utilisateurs;
using CollectManagement.Domain.Utilisateurs.Entities;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Features.RolesUtilisateur.Commands;

public class CreateRoleUtilisateurCommandHandlerTests
{
    private readonly Mock<IRoleUtilisateurRepository> _repository;

    public CreateRoleUtilisateurCommandHandlerTests()
    {
        _repository = new Mock<IRoleUtilisateurRepository>();
    }

    [Fact]
    public async Task Handle_Should_Create_Role()
    {
        var handler = new CreateRoleUtilisateurCommandHandler(_repository.Object);

        var command = new CreateRoleUtilisateurCommand(
            "Superviseur",
            new List<CreateRoleUtilisateurNavigation>());

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.RoleUtilisateurId.Should().NotBe(Ulid.Empty);

        _repository.Verify(
            x => x.AddAsync(
                It.IsAny<RoleUtilisateur>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
