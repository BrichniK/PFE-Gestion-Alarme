using CollectManagement.Application.Features.Groupes.Commands.DeleteGroupe;
using CollectManagement.Application.Interfaces.Groupes;
using Moq;

namespace CollectManagement.Tests.Features.Groupes.Commands;


public class DeleteGroupeCommandHandlerTests
{

    [Fact]
    public async Task Handle_Should_Delete_Groupe()
    {

        var repo = new Mock<IGroupeRepository>();


        var handler =
            new DeleteGroupeCommandHandler(repo.Object);


        var command = new DeleteGroupeCommand(
            Ulid.NewUlid()
        );


        await handler.Handle(
            command,
            CancellationToken.None
        );


        repo.Verify(
            x => x.DeleteAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<CollectManagement.Domain.Groupes.Groupe, bool>>>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

    }
}