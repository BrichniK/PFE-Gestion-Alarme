using CollectManagement.Application.Features.Utilisateurs.Commands.DeleteUtilisateur;
using CollectManagement.Application.Interfaces.Repositories.Utilisateurs;
using CollectManagement.Domain.Utilisateurs;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Features.Utilisateurs.Commands;

public class DeleteUtilisateurCommandHandlerTests
{
    private readonly Mock<IUtilisateurRepository> _repository;

    public DeleteUtilisateurCommandHandlerTests()
    {
        _repository = new Mock<IUtilisateurRepository>();
    }

    [Fact]
    public async Task Handle_Should_Delete_Utilisateur()
    {
        _repository
            .Setup(x => x.DeleteAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<Utilisateur, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeleteUtilisateurCommandHandler(_repository.Object);
        var command = new DeleteUtilisateurCommand(Ulid.NewUlid());

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().NotThrowAsync();

        _repository.Verify(
            x => x.DeleteAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<Utilisateur, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
