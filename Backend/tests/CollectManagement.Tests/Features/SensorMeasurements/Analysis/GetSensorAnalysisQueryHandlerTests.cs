using CollectManagement.Application.Features.SensorMeasurements.Analysis;
using CollectManagement.Application.Interfaces.Repositories.SensorMeasurements;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.SensorMeasurements;
using CollectManagement.Domain.SensorMeasurements.ValueObjects;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Features.SensorMeasurements.Analysis;

public class GetSensorAnalysisQueryHandlerTests
{
    private readonly Mock<ISensorMeasurementRepository> _repository;

    public GetSensorAnalysisQueryHandlerTests()
    {
        _repository = new Mock<ISensorMeasurementRepository>();
    }

    [Fact]
    public async Task Handle_Should_Return_NoData_When_No_Measurements_Exist()
    {
        // Arrange
        var deviceId = Ulid.NewUlid();

        _repository
            .Setup(x => x.GetForAnalysisAsync(
                deviceId,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new List<SensorMeasurement>());

        var handler = new GetSensorAnalysisQueryHandler(
            _repository.Object);

        var query = new GetSensorAnalysisQuery(deviceId);

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        result.DeviceId.Should().Be(deviceId);

        result.MeasurementCount.Should().Be(0);

        result.FailureCount.Should().Be(0);

        result.FailureRate.Should().Be(0);

        result.GlobalTrend.Should().Be("NoData");

        result.RiskLevel.Should().Be("Unknown");

        result.Recommendation.Should()
            .Contain("Aucune donnée disponible");
    }

    [Fact]
    public async Task Handle_Should_Analyze_Measurements()
    {
        // Arrange
        var deviceId = Ulid.NewUlid();

        var measurements = CreateMeasurements(
            deviceId,
            20);

        _repository
            .Setup(x => x.GetForAnalysisAsync(
                deviceId,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(measurements);

        var handler = new GetSensorAnalysisQueryHandler(
            _repository.Object);

        var query = new GetSensorAnalysisQuery(deviceId);

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        result.DeviceId.Should().Be(deviceId);

        result.MeasurementCount.Should().Be(20);

        result.Temperature.Should().NotBeNull();

        result.Vibration.Should().NotBeNull();

        result.Pressure.Should().NotBeNull();

        result.Humidity.Should().NotBeNull();

        result.Temperature.Average.Should().NotBeNull();

        result.Vibration.Average.Should().NotBeNull();

        result.Pressure.Average.Should().NotBeNull();

        result.Humidity.Average.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_Should_Count_Failures_And_Calculate_FailureRate()
    {
        // Arrange
        var deviceId = Ulid.NewUlid();

        var measurements = CreateMeasurements(
            deviceId,
            20);

        // 5 échecs sur 20 = 25 %
        for (var i = 0; i < 5; i++)
        {
            measurements[i] = SensorMeasurement.Create(
                new SensorMeasurementId(Ulid.NewUlid()),
                new DeviceId(deviceId),
                "A1",
                DateTime.UtcNow.AddMinutes(i),
                50,
                2,
                1000,
                50,
                true);
        }

        _repository
            .Setup(x => x.GetForAnalysisAsync(
                deviceId,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(measurements);

        var handler = new GetSensorAnalysisQueryHandler(
            _repository.Object);

        // Act
        var result = await handler.Handle(
            new GetSensorAnalysisQuery(deviceId),
            CancellationToken.None);

        // Assert
        result.FailureCount.Should().Be(5);

        result.FailureRate.Should().Be(25);
    }

    [Fact]
    public async Task Handle_Should_Pass_SensorCode_To_Repository()
    {
        // Arrange
        var deviceId = Ulid.NewUlid();

        _repository
            .Setup(x => x.GetForAnalysisAsync(
                deviceId,
                "A1",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                CreateMeasurements(deviceId, 10));

        var handler = new GetSensorAnalysisQueryHandler(
            _repository.Object);

        var query = new GetSensorAnalysisQuery(
            deviceId,
            "A1");

        // Act
        await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        _repository.Verify(
            x => x.GetForAnalysisAsync(
                deviceId,
                "A1",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Pass_CancellationToken_To_Repository()
    {
        // Arrange
        var deviceId = Ulid.NewUlid();

        using var cts = new CancellationTokenSource();

        var cancellationToken = cts.Token;

        _repository
            .Setup(x => x.GetForAnalysisAsync(
                deviceId,
                null,
                cancellationToken))
            .ReturnsAsync(
                new List<SensorMeasurement>());

        var handler = new GetSensorAnalysisQueryHandler(
            _repository.Object);

        var query = new GetSensorAnalysisQuery(deviceId);

        // Act
        await handler.Handle(
            query,
            cancellationToken);

        // Assert
        _repository.Verify(
            x => x.GetForAnalysisAsync(
                deviceId,
                null,
                cancellationToken),
            Times.Once);
    }

    private static List<SensorMeasurement> CreateMeasurements(
        Ulid deviceId,
        int count)
    {
        var measurements = new List<SensorMeasurement>();

        for (var i = 0; i < count; i++)
        {
            measurements.Add(
                SensorMeasurement.Create(
                    new SensorMeasurementId(Ulid.NewUlid()),
                    new DeviceId(deviceId),
                    "A1",
                    DateTime.UtcNow.AddMinutes(i),
                    50 + i,
                    2 + i * 0.1,
                    1000 + i,
                    50 + i,
                    false));
        }

        return measurements;
    }
}