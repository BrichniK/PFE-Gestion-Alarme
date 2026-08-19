using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.SMS.ValueObjects;
using FluentAssertions;
using SMSEntity = CollectManagement.Domain.SMS.SMS;

namespace CollectManagement.Tests.Features.Domain.SMS;

public class SMSTests
{
    [Fact]
    public void Create_Should_Create_SMS()
    {
        var id       = new SMSId(Ulid.NewUlid());
        var deviceId = new DeviceId(Ulid.NewUlid());

        var sms = SMSEntity.Create(
            id,
            "Ben Ali Mohamed",
            "21612345678",
            new[] { deviceId });

        sms.Should().NotBeNull();
        sms.SMSId.Should().Be(id);
        sms.NomPrenom.Should().Be("Ben Ali Mohamed");
        sms.PhoneNumber.Should().Be("21612345678");
        sms.SMSDevices.Should().HaveCount(1);
    }

    [Fact]
    public void Create_Deduplicates_DeviceIds()
    {
        var shared = new DeviceId(Ulid.NewUlid());

        var sms = SMSEntity.Create(
            new SMSId(Ulid.NewUlid()),
            "Test",
            "21600000000",
            new[] { shared, shared });

        sms.SMSDevices.Should().HaveCount(1);
    }

    [Fact]
    public void Update_Should_Modify_SMS()
    {
        var sms = SMSEntity.Create(
            new SMSId(Ulid.NewUlid()),
            "Old Name",
            "21600000000",
            new[] { new DeviceId(Ulid.NewUlid()) });

        var newDevice = new DeviceId(Ulid.NewUlid());

        sms.Update("New Name", "21699999999", new[] { newDevice });

        sms.NomPrenom.Should().Be("New Name");
        sms.PhoneNumber.Should().Be("21699999999");
        sms.SMSDevices.Should().HaveCount(1);
    }
}
