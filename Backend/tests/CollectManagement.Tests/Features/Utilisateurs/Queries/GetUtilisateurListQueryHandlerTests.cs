using CollectManagement.Application.Features.Utilisateurs.Queries.GetUtilisateurList;
using CollectManagement.Application.Interfaces.Repositories.Utilisateurs;
using CollectManagement.Domain.Utilisateurs;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Features.Utilisateurs.Queries;

public class GetUtilisateurListQueryHandlerTests
{
    private readonly Mock<IUtilisateurRepository> _repository;
    private readonly GetUtilisateurListQueryHandler _handler;

    public GetUtilisateurListQueryHandlerTests()
    {
        _repository = new Mock<IUtilisateurRepository>();
        _handler    = new GetUtilisateurListQueryHandler(_repository.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List()
    {
        _repository
            .Setup(x => x.GetPagedListAsync(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Utilisateur>().AsReadOnly() as IReadOnlyList<Utilisateur>, 0));

        var query  = new GetUtilisateurListQuery(null, null, null, 1, 10);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Utilisateurs.Should().BeEmpty();
        result.Length.Should().Be(0);
    }
}
