using CollectManagement.Application.Features.RolesUtilisateur.Queries.GetListRoleUtilisateur;
using CollectManagement.Application.Interfaces.Repositories.Utilisateurs;
using CollectManagement.Domain.Utilisateurs.Entities;
using CollectManagement.Domain.Utilisateurs.ValueObjects;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Features.RolesUtilisateur.Queries;

public class GetListRoleUtilisateurQueryHandlerTests
{
    private readonly Mock<IRoleUtilisateurRepository> _repository;
    private readonly GetListRoleUtilisateurQueryHandler _handler;

    public GetListRoleUtilisateurQueryHandlerTests()
    {
        _repository = new Mock<IRoleUtilisateurRepository>();
        _handler    = new GetListRoleUtilisateurQueryHandler(_repository.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Paged_List()
    {
        var roleId = new RoleUtilisateurId(Ulid.NewUlid());
        var role   = RoleUtilisateur.Create(roleId, "Technicien", new List<Navigation>());

        _repository
            .Setup(x => x.GetPagedListAsync(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<RoleUtilisateur> { role }, 1));

        var query  = new GetListRoleUtilisateurQuery(null, null, null, 1, 10);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Length.Should().Be(1);
        result.RolesUtilisateur.Should().HaveCount(1);
        result.RolesUtilisateur[0].LibelleRoleUtilisateur.Should().Be("Technicien");
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_When_No_Data()
    {
        _repository
            .Setup(x => x.GetPagedListAsync(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<RoleUtilisateur>(), 0));

        var result = await _handler.Handle(
            new GetListRoleUtilisateurQuery(null, null, null, 1, 10),
            CancellationToken.None);

        result.Length.Should().Be(0);
        result.RolesUtilisateur.Should().BeEmpty();
    }
}
