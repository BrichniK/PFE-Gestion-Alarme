using Moq;
using FluentAssertions;
using CollectManagement.Application.Features.Societes.Commands.DeleteSociete;
using CollectManagement.Application.Interfaces.Societes;

namespace CollectManagement.Tests.Features.Societes.Commands;


public class DeleteSocieteCommandHandlerTests
{

    [Fact]
    public async Task Handle_Should_Delete_Societe()
    {

        var repo = new Mock<ISocieteRepository>();


        var handler =
            new DeleteSocieteCommandHandler(repo.Object);


        var command = new DeleteSocieteCommand(
            Ulid.NewUlid()
        );


        await handler.Handle(
            command,
            CancellationToken.None
        );


        repo.Verify(
            x=>x.DeleteAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<CollectManagementDomain.Societes.Societe,bool>>>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

    }

}