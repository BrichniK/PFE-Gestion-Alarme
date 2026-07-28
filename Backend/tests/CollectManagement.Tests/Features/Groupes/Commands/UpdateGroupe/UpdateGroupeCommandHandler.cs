using CollectManagement.Application.Features.Groupes.Commands.UpdateGroupe;
using CollectManagement.Application.Interfaces.Groupes;
using Moq;

namespace CollectManagement.Tests.Features.Groupes.Commands;


public class UpdateGroupeCommandHandlerTests
{

    [Fact]
    public async Task Handle_Should_Update_Groupe()
    {

        var repository =
            new Mock<IGroupeRepository>();


        repository
            .Setup(x => x.UpdateBulkAsync(
                It.IsAny<CollectManagement.Domain.Groupes.Groupe>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);



        var handler =
            new UpdateGroupeCommandHandler(
                repository.Object);



        var command =
            new UpdateGroupeCommand(
                Ulid.NewUlid(),
                "Equipe Updated",
                "#00FF00",
                new List<Ulid>());



        await handler.Handle(
            command,
            CancellationToken.None);



        repository.Verify(
            x => x.UpdateBulkAsync(
                It.IsAny<CollectManagement.Domain.Groupes.Groupe>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

    }
}