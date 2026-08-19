using CollectManagement.Application.Exceptions;
using CollectManagement.Application.Features.RolesUtilisateur.Queries.GetOneRoleUtilisateur;
using CollectManagement.Application.Interfaces.Repositories.Utilisateurs;
using CollectManagement.Domain.Utilisateurs.Entities;
using CollectManagement.Domain.Utilisateurs.ValueObjects;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Features.RolesUtilisateur.Queries;

public class GetOneRoleUtilisateurQueryHandlerTests
{
    private readonly Mock<IRoleUtilisateurRepository> _repository;
    private readonly GetOneRoleUtilisateurQueryHandler _handler;

    public GetOneRoleUtilisateurQueryHandlerTests()
    {
        _repository = new Mock<IRoleUtilisateurRepository>();
        _handler    = new GetOneRoleUtilisateurQueryHandler(_repository.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Role_When_Found()
    {
        var roleId = new RoleUtilisateurId(Ulid.NewUlid());
        var role   = RoleUtilisateur.Create(roleId, "Superviseur", new List<Navigation>());

        _repository
            .Setup(x => x.GetOneAsync(
                It.IsAny<RoleUtilisateurId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        var query  = new GetOneRoleUtilisateurQuery(roleId.Value);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.RoleUtilisateurId.Should().Be(roleId.Value);
        result.LibelleRoleUtilisateur.Should().Be("Superviseur");
        result.Navigations.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Not_Found()
    {
        _repository
            .Setup(x => x.GetOneAsync(
                It.IsAny<RoleUtilisateurId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RoleUtilisateur?)null);

        var query = new GetOneRoleUtilisateurQuery(Ulid.NewUlid());

        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
