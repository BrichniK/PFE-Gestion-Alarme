using CollectManagement.Application.Features.Types.Commands.UpdateType;
using CollectManagement.Application.Interfaces.Repositories.Types;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Features.Types.Commands;

public class UpdateTypeCommandHandlerTests
{
    private readonly Mock<ITypeRepository> _repository;
    private readonly UpdateTypeCommandHandler _handler;


    public UpdateTypeCommandHandlerTests()
    {
        _repository = new Mock<ITypeRepository>();

        _handler = new UpdateTypeCommandHandler(
            _repository.Object);
    }



    [Fact]
    public async Task Handle_Should_Update_Type()
    {
        var id = Ulid.NewUlid();


        var command = new UpdateTypeCommand(
            id,
            "ELEC",
            "Electricite",
            45
        );



        await _handler.Handle(
            command,
            CancellationToken.None);



        _repository.Verify(
            x => x.UpdateBulkAsync(
                It.IsAny<CollectManagement.Domain.Types.Type>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}