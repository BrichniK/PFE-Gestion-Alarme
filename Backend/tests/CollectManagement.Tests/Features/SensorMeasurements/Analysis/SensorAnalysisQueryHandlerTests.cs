using CollectManagement.Application.Features.SensorMeasurements.Analysis;
using CollectManagement.Application.Interfaces.Repositories.SensorMeasurements;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.SensorMeasurements;
using CollectManagement.Domain.SensorMeasurements.ValueObjects;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Features.SensorMeasurements.Analysis;

public class SensorAnalysisQueryHandlerTests
{
    private readonly Mock<ISensorMeasurementRepository> _repository;
    private readonly GetSensorAnalysisQueryHandler _handler;

    public SensorAnalysisQueryHandlerTests()
    {
        _repository = new Mock<ISensorMeasurementRepository>();

        _handler = new GetSensorAnalysisQueryHandler(
            _repository.Object);
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

        var query = new GetSensorAnalysisQuery(deviceId);

        // Act
        var result = await _handler.Handle(
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

        result.Recommendation
            .Should()
            .Be("Aucune donnée disponible pour cette machine.");

        result.Temperature.Trend.Should().Be("NoData");
        result.Vibration.Trend.Should().Be("NoData");
        result.Pressure.Trend.Should().Be("NoData");
        result.Humidity.Trend.Should().Be("NoData");

        _repository.Verify(
            x => x.GetForAnalysisAsync(
                deviceId,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Calculate_Basic_Statistics()
    {
        // Arrange
        var deviceId = Ulid.NewUlid();

        var measurements = CreateMeasurements(
            deviceId,
            count: 20,
            temperature: i => 20 + i,
            vibration: _ => 1,
            pressure: _ => 1000,
            humidity: _ => 50);

        _repository
            .Setup(x => x.GetForAnalysisAsync(
                deviceId,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(measurements);

        var query = new GetSensorAnalysisQuery(deviceId);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.MeasurementCount.Should().Be(20);

        result.FailureCount.Should().Be(0);

        result.FailureRate.Should().Be(0);

        result.Temperature.Average.Should().Be(29.5);

        result.Temperature.Minimum.Should().Be(20);

        result.Temperature.Maximum.Should().Be(39);
    }

    [Fact]
    public async Task Handle_Should_Calculate_Failure_Rate()
    {
        // Arrange
        var deviceId = Ulid.NewUlid();

        var measurements = CreateMeasurements(
            deviceId,
            count: 20,
            temperature: _ => 20,
            vibration: _ => 1,
            pressure: _ => 1000,
            humidity: _ => 50);

        // 5 failures / 20 = 25 %
        for (var i = 0; i < 5; i++)
        {
            measurements[i] = SensorMeasurement.Create(
                new SensorMeasurementId(Ulid.NewUlid()),
                new DeviceId(deviceId),
                "A1",
                DateTime.UtcNow.AddMinutes(i),
                20,
                1,
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

        var query = new GetSensorAnalysisQuery(deviceId);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.FailureCount.Should().Be(5);

        result.FailureRate.Should().Be(25);
    }

    [Fact]
    public async Task Handle_Should_Detect_Increasing_Trend()
    {
        // Arrange
        var deviceId = Ulid.NewUlid();

        /*
         * 50 mesures historiques à faible valeur
         * puis 10 mesures récentes à valeur élevée.
         */
        var measurements = new List<SensorMeasurement>();

        for (var i = 0; i < 50; i++)
        {
            measurements.Add(
                CreateMeasurement(
                    deviceId,
                    i,
                    temperature: 20,
                    vibration: 1,
                    pressure: 1000,
                    humidity: 50));
        }

        for (var i = 50; i < 60; i++)
        {
            measurements.Add(
                CreateMeasurement(
                    deviceId,
                    i,
                    temperature: 30,
                    vibration: 1,
                    pressure: 1000,
                    humidity: 50));
        }

        _repository
            .Setup(x => x.GetForAnalysisAsync(
                deviceId,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(measurements);

        var query = new GetSensorAnalysisQuery(deviceId);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Temperature.Trend
            .Should()
            .Be("Increasing");

        result.Temperature.VariationPercentage
            .Should()
            .Be(50);
    }

    [Fact]
    public async Task Handle_Should_Detect_Decreasing_Trend()
    {
        // Arrange
        var deviceId = Ulid.NewUlid();

        var measurements = new List<SensorMeasurement>();

        for (var i = 0; i < 50; i++)
        {
            measurements.Add(
                CreateMeasurement(
                    deviceId,
                    i,
                    temperature: 30,
                    vibration: 1,
                    pressure: 1000,
                    humidity: 50));
        }

        for (var i = 50; i < 60; i++)
        {
            measurements.Add(
                CreateMeasurement(
                    deviceId,
                    i,
                    temperature: 20,
                    vibration: 1,
                    pressure: 1000,
                    humidity: 50));
        }

        _repository
            .Setup(x => x.GetForAnalysisAsync(
                deviceId,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(measurements);

        var query = new GetSensorAnalysisQuery(deviceId);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Temperature.Trend
            .Should()
            .Be("Decreasing");

        result.Temperature.VariationPercentage
            .Should()
            .Be(-33.33);
    }

    [Fact]
    public async Task Handle_Should_Detect_Stable_Trend()
    {
        // Arrange
        var deviceId = Ulid.NewUlid();

        var measurements = CreateMeasurements(
            deviceId,
            60,
            _ => 20,
            _ => 1,
            _ => 1000,
            _ => 50);

        _repository
            .Setup(x => x.GetForAnalysisAsync(
                deviceId,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(measurements);

        var query = new GetSensorAnalysisQuery(deviceId);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Temperature.Trend.Should().Be("Stable");
        result.Vibration.Trend.Should().Be("Stable");
        result.Pressure.Trend.Should().Be("Stable");
        result.Humidity.Trend.Should().Be("Stable");

        result.GlobalTrend.Should().Be("Stable");

        result.RiskLevel.Should().Be("Low");

        result.Recommendation
            .Should()
            .Contain("stables");
    }

    [Fact]
    public async Task Handle_Should_Return_Degradation_When_Two_Metrics_Increase()
    {
        // Arrange
        var deviceId = Ulid.NewUlid();

        var measurements = new List<SensorMeasurement>();

        for (var i = 0; i < 50; i++)
        {
            measurements.Add(
                CreateMeasurement(
                    deviceId,
                    i,
                    temperature: 20,
                    vibration: 1,
                    pressure: 1000,
                    humidity: 50));
        }

        for (var i = 50; i < 60; i++)
        {
            measurements.Add(
                CreateMeasurement(
                    deviceId,
                    i,
                    temperature: 30,
                    vibration: 2,
                    pressure: 1000,
                    humidity: 50));
        }

        _repository
            .Setup(x => x.GetForAnalysisAsync(
                deviceId,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(measurements);

        var query = new GetSensorAnalysisQuery(deviceId);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Temperature.Trend
            .Should()
            .Be("Increasing");

        result.Vibration.Trend
            .Should()
            .Be("Increasing");

        result.GlobalTrend
            .Should()
            .Be("Degradation");

        /*
         * IMPORTANT :
         * On vérifie le comportement ACTUEL
         * du code production.
         *
         * RiskLevel = Low dans ce scénario :
         * 2 indicateurs augmentent => score 2.
         */
        result.RiskLevel
            .Should()
            .Be("Low");

        result.Recommendation
            .Should()
            .Be("Maintenir la surveillance de la machine.");
    }

    [Fact]
    public async Task Handle_Should_Return_Improvement_When_Two_Metrics_Decrease()
    {
        // Arrange
        var deviceId = Ulid.NewUlid();

        var measurements = new List<SensorMeasurement>();

        for (var i = 0; i < 50; i++)
        {
            measurements.Add(
                CreateMeasurement(
                    deviceId,
                    i,
                    temperature: 30,
                    vibration: 2,
                    pressure: 1000,
                    humidity: 50));
        }

        for (var i = 50; i < 60; i++)
        {
            measurements.Add(
                CreateMeasurement(
                    deviceId,
                    i,
                    temperature: 20,
                    vibration: 1,
                    pressure: 1000,
                    humidity: 50));
        }

        _repository
            .Setup(x => x.GetForAnalysisAsync(
                deviceId,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(measurements);

        var query = new GetSensorAnalysisQuery(deviceId);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Temperature.Trend
            .Should()
            .Be("Decreasing");

        result.Vibration.Trend
            .Should()
            .Be("Decreasing");

        result.GlobalTrend
            .Should()
            .Be("Improvement");

        result.RiskLevel
            .Should()
            .Be("Low");

        result.Recommendation
            .Should()
            .Contain("amélioration");
    }

    [Fact]
    public async Task Handle_Should_Return_Moderate_Risk_When_Failure_Rate_Is_At_Least_5_Percent()
    {
        // Arrange
        var deviceId = Ulid.NewUlid();

        var measurements = CreateMeasurements(
            deviceId,
            60,
            _ => 20,
            _ => 1,
            _ => 1000,
            _ => 50);

        for (var i = 0; i < 5; i++)
        {
            measurements[i] = SensorMeasurement.Create(
                new SensorMeasurementId(Ulid.NewUlid()),
                new DeviceId(deviceId),
                "A1",
                DateTime.UtcNow.AddMinutes(i),
                20,
                1,
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

        var query = new GetSensorAnalysisQuery(deviceId);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.FailureRate.Should().Be(8.33);

        result.RiskLevel.Should().Be("Moderate");

        result.Recommendation
            .Should()
            .Contain("Risque modéré");
    }

    [Fact]
    public async Task Handle_Should_Return_High_Risk_When_Failure_Rate_And_Increasing_Metrics_Are_High()
    {
        // Arrange
        var deviceId = Ulid.NewUlid();

        var measurements = new List<SensorMeasurement>();

        for (var i = 0; i < 50; i++)
        {
            measurements.Add(
                CreateMeasurement(
                    deviceId,
                    i,
                    temperature: 20,
                    vibration: 1,
                    pressure: 1000,
                    humidity: 50,
                    isFailure: i < 3));
        }

        for (var i = 50; i < 60; i++)
        {
            measurements.Add(
                CreateMeasurement(
                    deviceId,
                    i,
                    temperature: 40,
                    vibration: 3,
                    pressure: 1100,
                    humidity: 50,
                    isFailure: true));
        }

        _repository
            .Setup(x => x.GetForAnalysisAsync(
                deviceId,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(measurements);

        var query = new GetSensorAnalysisQuery(deviceId);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.RiskLevel.Should().Be("High");

        result.Recommendation
            .Should()
            .Contain("Risque élevé");

        result.Recommendation
            .Should()
            .Contain("température");

        result.Recommendation
            .Should()
            .Contain("vibration");
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
                new List<SensorMeasurement>());

        var query = new GetSensorAnalysisQuery(
            deviceId,
            "A1");

        // Act
        await _handler.Handle(
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

        var query = new GetSensorAnalysisQuery(deviceId);

        // Act
        await _handler.Handle(
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

    [Fact]
    public async Task Handle_Should_Ignore_Null_Sensor_Values()
    {
        // Arrange
        var deviceId = Ulid.NewUlid();

        var measurements = new List<SensorMeasurement>();

        for (var i = 0; i < 60; i++)
        {
            measurements.Add(
                SensorMeasurement.Create(
                    new SensorMeasurementId(Ulid.NewUlid()),
                    new DeviceId(deviceId),
                    "A1",
                    DateTime.UtcNow.AddMinutes(i),
                    i < 50 ? 20 : 30,
                    i < 50 ? 1 : 2,
                    null,
                    null,
                    false));
        }

        _repository
            .Setup(x => x.GetForAnalysisAsync(
                deviceId,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(measurements);

        var query = new GetSensorAnalysisQuery(deviceId);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Temperature.Trend
            .Should()
            .Be("Increasing");

        result.Vibration.Trend
            .Should()
            .Be("Increasing");

        result.Pressure.Trend
            .Should()
            .Be("NoData");

        result.Humidity.Trend
            .Should()
            .Be("NoData");

        result.GlobalTrend
            .Should()
            .Be("Degradation");
    }

    private static SensorMeasurement CreateMeasurement(
        Ulid deviceId,
        int index,
        double? temperature,
        double? vibration,
        double? pressure,
        double? humidity,
        bool isFailure = false)
    {
        return SensorMeasurement.Create(
            new SensorMeasurementId(Ulid.NewUlid()),
            new DeviceId(deviceId),
            "A1",
            DateTime.UtcNow.AddMinutes(index),
            temperature,
            vibration,
            pressure,
            humidity,
            isFailure);
    }

    private static List<SensorMeasurement> CreateMeasurements(
        Ulid deviceId,
        int count,
        Func<int, double?> temperature,
        Func<int, double?> vibration,
        Func<int, double?> pressure,
        Func<int, double?> humidity)
    {
        var measurements = new List<SensorMeasurement>();

        for (var i = 0; i < count; i++)
        {
            measurements.Add(
                CreateMeasurement(
                    deviceId,
                    i,
                    temperature(i),
                    vibration(i),
                    pressure(i),
                    humidity(i)));
        }

        return measurements;
    }
}