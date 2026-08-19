using CollectManagement.Application.Exceptions;
using CollectManagement.Application.Features.RolesUtilisateur.Commands.UpdateRoleUtilisateur;
using CollectManagement.Application.Interfaces.Repositories.Utilisateurs;
using CollectManagement.Domain.Utilisateurs.Entities;
using CollectManagement.Domain.Utilisateurs.ValueObjects;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Features.RolesUtilisateur.Commands;

public class UpdateRoleUtilisateurCommandHandlerTests
{
    private readonly Mock<IRoleUtilisateurRepository> _repository;
    private readonly UpdateRoleUtilisateurCommandHandler _handler;

    public UpdateRoleUtilisateurCommandHandlerTests()
    {
        _repository = new Mock<IRoleUtilisateurRepository>();
        _handler    = new UpdateRoleUtilisateurCommandHandler(_repository.Object);
    }

    [Fact]
    public async Task Handle_Should_Update_Role_When_Found()
    {
        var roleId = new RoleUtilisateurId(Ulid.NewUlid());
        var role   = RoleUtilisateur.Create(roleId, "OldLibelle", new List<Navigation>());

        _repository
            .Setup(x => x.GetOneAsync(
                It.IsAny<RoleUtilisateurId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        var command = new UpdateRoleUtilisateurCommand(
            roleId.Value,
            "NewLibelle",
            new List<UpdateRoleUtilisateurNavigation>());

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().NotThrowAsync();

        _repository.Verify(x => x.Attach(It.IsAny<RoleUtilisateur>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Not_Found()
    {
        _repository
            .Setup(x => x.GetOneAsync(
                It.IsAny<RoleUtilisateurId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RoleUtilisateur?)null);

        var command = new UpdateRoleUtilisateurCommand(
            Ulid.NewUlid(),
            "LibelleX",
            new List<UpdateRoleUtilisateurNavigation>());

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
