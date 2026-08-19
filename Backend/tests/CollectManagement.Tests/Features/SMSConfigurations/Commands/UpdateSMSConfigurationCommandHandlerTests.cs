using CollectManagement.Application.Features.SMSConfigurations.Commands.UpdateSMSConfiguration;
using CollectManagement.Application.Interfaces.Repositories.SMSConfigurations;
using CollectManagement.Domain.SMSConfigurations;
using CollectManagement.Domain.SMSConfigurations.ValueObjects;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Features.SMSConfigurations.Commands;

public class UpdateSMSConfigurationCommandHandlerTests
{

    private readonly Mock<ISMSConfigurationRepository> _repository;
    private readonly UpdateSMSConfigurationCommandHandler _handler;


    public UpdateSMSConfigurationCommandHandlerTests()
    {
        _repository = new Mock<ISMSConfigurationRepository>();
        _handler    = new UpdateSMSConfigurationCommandHandler(_repository.Object);
    }


    [Fact]
    public async Task Handle_Should_Create_Config_When_None_Exists()
    {

        _repository
            .Setup(x => x.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((SMSConfiguration?)null);

        var command = new UpdateSMSConfigurationCommand(
            "http://192.168.1.22/api/sms",
            true, 3, 60,
            true, true, true, true, true
        );


        var result = await _handler.Handle(command, CancellationToken.None);


        result.Should().NotBeNull();

        result.SMSConfigurationId.Should().NotBe(Ulid.Empty);

        _repository.Verify(
            x => x.AddAsync(It.IsAny<SMSConfiguration>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task Handle_Should_Update_Config_When_Exists()
    {

        var configId = new SMSConfigurationId(Ulid.NewUlid());

        var existing = SMSConfiguration.Create(
            configId,
            "http://old-url/api/sms",
            false, 1, 30,
            false, false, false, false, false
        );

        _repository
            .Setup(x => x.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _repository
            .Setup(x => x.UpdateBulkAsync(It.IsAny<SMSConfiguration>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new UpdateSMSConfigurationCommand(
            "http://new-url/api/sms",
            true, 5, 120,
            true, true, false, true, false
        );


        var result = await _handler.Handle(command, CancellationToken.None);


        result.Should().NotBeNull();

        result.SMSConfigurationId.Should().Be(configId.Value);

        _repository.Verify(
            x => x.UpdateBulkAsync(It.IsAny<SMSConfiguration>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
