using CollectManagement.Application.Interfaces.Repositories.SensorMeasurements;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.SensorMeasurements;
using CollectManagement.Domain.SensorMeasurements.ValueObjects;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Infrastructure.Repositories;

public class SensorMeasurementRepositoryTests
{

    private readonly Mock<ISensorMeasurementRepository> _repository;


    public SensorMeasurementRepositoryTests()
    {
        _repository = new Mock<ISensorMeasurementRepository>();
    }


    [Fact]
    public async Task GetOneAsync_Should_Return_SensorMeasurement()
    {

        var id = new SensorMeasurementId(Ulid.NewUlid());

        var sm = SensorMeasurement.Create(
            id,
            new DeviceId(Ulid.NewUlid()),
            "CAPTEUR-01",
            DateTime.UtcNow,
            25.5, 0.3, 1013.0, 60.0
        );

        _repository
            .Setup(x => x.GetOneAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sm);


        var result = await _repository.Object.GetOneAsync(id, CancellationToken.None);


        result.Should().NotBeNull();

        result!.SensorMeasurementId.Should().Be(id);

        result.SensorCode.Should().Be("CAPTEUR-01");
    }


    [Fact]
    public async Task GetForAnalysisAsync_Should_Return_List()
    {

        var deviceId = Ulid.NewUlid();

        var smList = new List<SensorMeasurement>
        {
            SensorMeasurement.Create(
                new SensorMeasurementId(Ulid.NewUlid()),
                new DeviceId(deviceId),
                "CAPTEUR-01",
                DateTime.UtcNow,
                22.0, null, null, null
            )
        };

        _repository
            .Setup(x => x.GetForAnalysisAsync(
                deviceId,
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(smList.AsReadOnly());


        var result = await _repository.Object.GetForAnalysisAsync(
            deviceId, null, CancellationToken.None);


        result.Should().NotBeNull();

        result.Should().HaveCount(1);

        result[0].Temperature.Should().Be(22.0);
    }
}
