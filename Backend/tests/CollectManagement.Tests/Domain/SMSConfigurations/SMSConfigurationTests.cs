using CollectManagement.Domain.SMSConfigurations;
using CollectManagement.Domain.SMSConfigurations.ValueObjects;
using FluentAssertions;

namespace CollectManagement.Tests.Features.Domain.SMSConfigurations;

public class SMSConfigurationTests
{
    [Fact]
    public void Create_Should_Create_SMSConfiguration()
    {
        var id = new SMSConfigurationId(Ulid.NewUlid());

        var config = SMSConfiguration.Create(
            id,
            "http://api/sms",
            true, 3, 60,
            true, false, true, false, true);

        config.Should().NotBeNull();
        config.SMSConfigurationId.Should().Be(id);
        config.ApiUrl.Should().Be("http://api/sms");
        config.IsActive.Should().BeTrue();
        config.NombreAlerte.Should().Be(3);
        config.Delai.Should().Be(60);
        config.SmsOnAlerte.Should().BeTrue();
        config.SmsOnBadgeT3.Should().BeFalse();
        config.SmsOnBadgeT4.Should().BeTrue();
        config.SmsOnTraitement.Should().BeTrue();
    }

    [Fact]
    public void Update_Should_Modify_SMSConfiguration()
    {
        var config = SMSConfiguration.Create(
            new SMSConfigurationId(Ulid.NewUlid()),
            "http://old/api", false, 1, 30,
            false, false, false, false, false);

        config.Update(
            "http://new/api", true, 5, 120,
            true, true, true, true, true);

        config.ApiUrl.Should().Be("http://new/api");
        config.IsActive.Should().BeTrue();
        config.NombreAlerte.Should().Be(5);
        config.Delai.Should().Be(120);
        config.SmsOnAlerte.Should().BeTrue();
        config.SmsOnBadgeT3.Should().BeTrue();
    }
}
