using CollectManagement.Application.Exceptions;
using CollectManagement.Application.Features.RolesUtilisateur.Commands.DeleteRoleUtilisateur;
using CollectManagement.Application.Interfaces.Repositories.Utilisateurs;
using CollectManagement.Domain.Utilisateurs.Entities;
using CollectManagement.Domain.Utilisateurs.ValueObjects;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Features.RolesUtilisateur.Commands;

public class DeleteRoleUtilisateurCommandHandlerTests
{
    private readonly Mock<IRoleUtilisateurRepository> _repository;
    private readonly DeleteRoleUtilisateurCommandHandler _handler;

    public DeleteRoleUtilisateurCommandHandlerTests()
    {
        _repository = new Mock<IRoleUtilisateurRepository>();
        _handler    = new DeleteRoleUtilisateurCommandHandler(_repository.Object);
    }

    [Fact]
    public async Task Handle_Should_Delete_Role_When_Found()
    {
        var roleId = new RoleUtilisateurId(Ulid.NewUlid());
        var role   = RoleUtilisateur.Create(roleId, "Admin", new List<Navigation>());

        _repository
            .Setup(x => x.GetOneAsync(
                It.IsAny<RoleUtilisateurId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _repository
            .Setup(x => x.DeleteAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<RoleUtilisateur, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var act = async () => await _handler.Handle(
            new DeleteRoleUtilisateurCommand(roleId.Value),
            CancellationToken.None);

        await act.Should().NotThrowAsync();

        _repository.Verify(
            x => x.DeleteAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<RoleUtilisateur, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Not_Found()
    {
        _repository
            .Setup(x => x.GetOneAsync(
                It.IsAny<RoleUtilisateurId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RoleUtilisateur?)null);

        var act = async () => await _handler.Handle(
            new DeleteRoleUtilisateurCommand(Ulid.NewUlid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
