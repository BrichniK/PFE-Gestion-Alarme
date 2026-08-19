using CollectManagement.Application.Features.SMSConfigurations.Queries.GetSMSConfiguration;
using CollectManagement.Application.Interfaces.Repositories.SMSConfigurations;
using CollectManagement.Domain.SMSConfigurations;
using CollectManagement.Domain.SMSConfigurations.ValueObjects;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Features.SMSConfigurations.Queries;

public class GetSMSConfigurationQueryHandlerTests
{
    private readonly Mock<ISMSConfigurationRepository> _repository;
    private readonly GetSMSConfigurationQueryHandler _handler;

    public GetSMSConfigurationQueryHandlerTests()
    {
        _repository = new Mock<ISMSConfigurationRepository>();
        _handler    = new GetSMSConfigurationQueryHandler(_repository.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Default_When_No_Config_Exists()
    {
        _repository
            .Setup(x => x.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((SMSConfiguration?)null);

        var result = await _handler.Handle(new GetSMSConfigurationQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result.SMSConfigurationId.Should().BeNull();
        result.ApiUrl.Should().BeEmpty();
        result.IsActive.Should().BeFalse();
        result.SmsOnAlerte.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_Return_Config_When_Exists()
    {
        var configId = new SMSConfigurationId(Ulid.NewUlid());
        var config   = SMSConfiguration.Create(
            configId,
            "http://api/sms",
            true, 3, 60,
            true, false, true, false, true);

        _repository
            .Setup(x => x.GetConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        var result = await _handler.Handle(new GetSMSConfigurationQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result.SMSConfigurationId.Should().Be(configId.Value);
        result.ApiUrl.Should().Be("http://api/sms");
        result.IsActive.Should().BeTrue();
        result.NombreAlerte.Should().Be(3);
        result.SmsOnBadgeT3.Should().BeFalse();

        _repository.Verify(
            x => x.GetConfigurationAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
