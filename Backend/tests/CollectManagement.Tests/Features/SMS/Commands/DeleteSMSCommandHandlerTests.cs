using CollectManagement.Application.Features.SMS.Commands.DeleteSMS;
using CollectManagement.Application.Interfaces.Repositories.SMS;
using FluentAssertions;
using Moq;
using SMSEntity = CollectManagement.Domain.SMS.SMS;

namespace CollectManagement.Tests.Features.SMS.Commands;

public class DeleteSMSCommandHandlerTests
{

    private readonly Mock<ISMSRepository> _repository;


    public DeleteSMSCommandHandlerTests()
    {
        _repository = new Mock<ISMSRepository>();
    }


    [Fact]
    public async Task Handle_Should_Delete_SMS()
    {

        var handler = new DeleteSMSCommandHandler(_repository.Object);

        var command = new DeleteSMSCommand(Ulid.NewUlid());


        var result = await handler.Handle(command, CancellationToken.None);


        result.Should().BeTrue();

        _repository.Verify(
            x => x.DeleteAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<SMSEntity, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
