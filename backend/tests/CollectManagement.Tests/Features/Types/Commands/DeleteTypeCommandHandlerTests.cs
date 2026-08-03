using CollectManagement.Application.Features.Types.Commands.DeleteType;
using CollectManagement.Application.Interfaces.Repositories.Types;
using FluentAssertions;
using Moq;
using Type = CollectManagement.Domain.Types.Type;

namespace CollectManagement.Tests.Features.Types.Commands;

public class DeleteTypeCommandHandlerTests
{
    [Fact]
    public async Task HandleShouldDeleteType()
    {

        var repository = new Mock<ITypeRepository>();


        var handler = new DeleteTypeCommandHandler(
            repository.Object);



        var command = new DeleteTypeCommand(
            Ulid.NewUlid()
        );



        await handler.Handle(
            command,
            CancellationToken.None);



        repository.Verify(
            x => x.DeleteAsync(
                It.IsAny<System.Linq.Expressions.Expression<
                    Func<Type, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}