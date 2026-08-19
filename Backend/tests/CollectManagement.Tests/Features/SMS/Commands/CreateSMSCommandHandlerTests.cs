using CollectManagement.Application.Features.SMS.Commands.CreateSMS;
using CollectManagement.Application.Interfaces.Repositories.SMS;
using FluentAssertions;
using Moq;
using SMSEntity = CollectManagement.Domain.SMS.SMS;

namespace CollectManagement.Tests.Features.SMS.Commands;

public class CreateSMSCommandHandlerTests
{

    private readonly Mock<ISMSRepository> _repository;


    public CreateSMSCommandHandlerTests()
    {
        _repository = new Mock<ISMSRepository>();
    }


    [Fact]
    public async Task Handle_Should_Create_SMS()
    {

        var handler = new CreateSMSCommandHandler(_repository.Object);

        var command = new CreateSMSCommand(
            "Ben Ali Mohamed",
            "21612345678",
            new List<Ulid> { Ulid.NewUlid() }
        );


        var result = await handler.Handle(command, CancellationToken.None);


        result.Should().NotBeNull();

        result.SMSId.Should().NotBe(Ulid.Empty);

        _repository.Verify(
            x => x.AddAsync(
                It.IsAny<SMSEntity>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
