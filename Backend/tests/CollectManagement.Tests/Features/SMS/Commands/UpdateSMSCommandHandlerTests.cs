using CollectManagement.Application.Features.SMS.Commands.UpdateSMS;
using CollectManagement.Application.Interfaces.Repositories.SMS;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.SMS.ValueObjects;
using FluentAssertions;
using Moq;
using SMSEntity = CollectManagement.Domain.SMS.SMS;

namespace CollectManagement.Tests.Features.SMS.Commands;

public class UpdateSMSCommandHandlerTests
{

    private readonly Mock<ISMSRepository> _repository;


    public UpdateSMSCommandHandlerTests()
    {
        _repository = new Mock<ISMSRepository>();
    }


    [Fact]
    public async Task Handle_Should_Update_SMS()
    {

        var smsId = Ulid.NewUlid();

        var existing = SMSEntity.Create(
            new SMSId(smsId),
            "Old Name",
            "21600000000",
            new[] { new DeviceId(Ulid.NewUlid()) }
        );

        _repository
            .Setup(x => x.GetOneAsync(
                It.IsAny<SMSId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _repository
            .Setup(x => x.UpdateBulkAsync(
                It.IsAny<SMSEntity>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new UpdateSMSCommandHandler(_repository.Object);

        var command = new UpdateSMSCommand(
            smsId,
            "New Name",
            "21699999999",
            new List<Ulid> { Ulid.NewUlid() }
        );


        var result = await handler.Handle(command, CancellationToken.None);


        result.Should().BeTrue();

        _repository.Verify(
            x => x.UpdateBulkAsync(
                It.IsAny<SMSEntity>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
