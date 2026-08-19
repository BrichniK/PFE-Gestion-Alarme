using CollectManagement.Application.Interfaces.Repositories.SMS;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.SMS.ValueObjects;
using FluentAssertions;
using Moq;
using SMSEntity = CollectManagement.Domain.SMS.SMS;

namespace CollectManagement.Tests.Infrastructure.Repositories;

public class SMSRepositoryTests
{
    private readonly Mock<ISMSRepository> _repository;

    public SMSRepositoryTests()
    {
        _repository = new Mock<ISMSRepository>();
    }

    [Fact]
    public async Task GetOneAsync_Should_Return_SMS()
    {
        var id  = new SMSId(Ulid.NewUlid());
        var sms = SMSEntity.Create(
            id, "Ben Ali", "21612345678",
            new[] { new DeviceId(Ulid.NewUlid()) });

        _repository
            .Setup(x => x.GetOneAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sms);

        var result = await _repository.Object.GetOneAsync(id, CancellationToken.None);

        result.Should().NotBeNull();
        result!.SMSId.Should().Be(id);
        result.NomPrenom.Should().Be("Ben Ali");
    }

    [Fact]
    public async Task GetByDeviceIdAsync_Should_Return_List()
    {
        var deviceId = new DeviceId(Ulid.NewUlid());
        var sms      = SMSEntity.Create(
            new SMSId(Ulid.NewUlid()), "Test", "21600000000",
            new[] { deviceId });

        _repository
            .Setup(x => x.GetByDeviceIdAsync(deviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SMSEntity> { sms }.AsReadOnly());

        var result = await _repository.Object.GetByDeviceIdAsync(deviceId, CancellationToken.None);

        result.Should().HaveCount(1);
    }
}
