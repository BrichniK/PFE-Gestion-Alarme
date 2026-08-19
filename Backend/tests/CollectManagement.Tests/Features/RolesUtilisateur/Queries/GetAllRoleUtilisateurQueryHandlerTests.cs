using CollectManagement.Application.Features.RolesUtilisateur.Queries.GetAllRoleUtilisateur;
using CollectManagement.Application.Interfaces.Repositories.Utilisateurs;
using CollectManagement.Domain.Utilisateurs.Entities;
using CollectManagement.Domain.Utilisateurs.ValueObjects;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Features.RolesUtilisateur.Queries;

public class GetAllRoleUtilisateurQueryHandlerTests
{
    private readonly Mock<IRoleUtilisateurRepository> _repository;
    private readonly GetAllRoleUtilisateurQueryHandler _handler;

    public GetAllRoleUtilisateurQueryHandlerTests()
    {
        _repository = new Mock<IRoleUtilisateurRepository>();
        _handler    = new GetAllRoleUtilisateurQueryHandler(_repository.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_All_Roles()
    {
        var roleId = new RoleUtilisateurId(Ulid.NewUlid());
        var role   = RoleUtilisateur.Create(roleId, "Admin", new List<Navigation>());

        _repository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleUtilisateur> { role });

        var result = await _handler.Handle(new GetAllRoleUtilisateurQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].RoleUtilisateurId.Should().Be(roleId.Value);
        result[0].LibelleRoleUtilisateur.Should().Be("Admin");
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Roles()
    {
        _repository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleUtilisateur>());

        var result = await _handler.Handle(new GetAllRoleUtilisateurQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
