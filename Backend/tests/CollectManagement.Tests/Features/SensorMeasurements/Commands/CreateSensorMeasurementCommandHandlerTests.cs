using CollectManagement.Application.Features.SensorMeasurements.Commands.CreateSensorMeasurement;
using CollectManagement.Application.Features.SensorMeasurements.Mapping;
using CollectManagement.Application.Interfaces.Repositories.SensorMeasurements;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.SensorMeasurements;
using FluentAssertions;
using Mapster;
using MapsterMapper;
using Moq;

namespace CollectManagement.Tests.Features.SensorMeasurements.Commands;

public class CreateSensorMeasurementCommandHandlerTests
{
    private readonly Mock<ISensorMeasurementRepository> _repository;
    private readonly IMapper _mapper;

    public CreateSensorMeasurementCommandHandlerTests()
    {
        _repository = new Mock<ISensorMeasurementRepository>();

        var config = new TypeAdapterConfig();

        new SensorMeasurementMapping().Register(config);

        _mapper = new Mapper(config);
    }

    [Fact]
    public async Task Handle_Should_Create_SensorMeasurement()
    {
        // Arrange
        var deviceId = Ulid.NewUlid();
        var date = DateTime.UtcNow;

        var command = new CreateSensorMeasurementCommand(
            deviceId,
            "CAPTEUR-01",
            date,
            25.5,
            0.3,
            1013.0,
            60.0,
            false);

        var handler = new CreateSensorMeasurementCommandHandler(
            _repository.Object,
            _mapper);

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        result.SensorMeasurementId
            .Should()
            .NotBe(Ulid.Empty);

        result.SensorCode
            .Should()
            .Be("CAPTEUR-01");

        result.Temperature
            .Should()
            .Be(25.5);

        result.Vibration
            .Should()
            .Be(0.3);

        result.Pressure
            .Should()
            .Be(1013.0);

        result.Humidity
            .Should()
            .Be(60.0);

        result.IsFailure
            .Should()
            .BeFalse();

        _repository.Verify(
            x => x.AddAsync(
                It.IsAny<SensorMeasurement>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Create_SensorMeasurement_With_IsFailure_True()
    {
        // Arrange
        var command = new CreateSensorMeasurementCommand(
            Ulid.NewUlid(),
            "CAPTEUR-02",
            DateTime.UtcNow,
            null,
            null,
            null,
            null,
            true);

        var handler = new CreateSensorMeasurementCommandHandler(
            _repository.Object,
            _mapper);

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        result.SensorMeasurementId
            .Should()
            .NotBe(Ulid.Empty);

        result.SensorCode
            .Should()
            .Be("CAPTEUR-02");

        result.Temperature
            .Should()
            .BeNull();

        result.Vibration
            .Should()
            .BeNull();

        result.Pressure
            .Should()
            .BeNull();

        result.Humidity
            .Should()
            .BeNull();

        result.IsFailure
            .Should()
            .BeTrue();

        _repository.Verify(
            x => x.AddAsync(
                It.IsAny<SensorMeasurement>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Map_All_Values_Correctly()
    {
        // Arrange
        var deviceId = Ulid.NewUlid();
        var measuredAt = DateTime.UtcNow;

        var command = new CreateSensorMeasurementCommand(
            deviceId,
            "A1",
            measuredAt,
            25.5,
            0.35,
            1013.5,
            62.5,
            true);

        SensorMeasurement? capturedMeasurement = null;

        _repository
            .Setup(x => x.AddAsync(
                It.IsAny<SensorMeasurement>(),
                It.IsAny<CancellationToken>()))
            .Callback<SensorMeasurement, CancellationToken>(
                (measurement, _) =>
                {
                    capturedMeasurement = measurement;
                });

        var handler = new CreateSensorMeasurementCommandHandler(
            _repository.Object,
            _mapper);

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        capturedMeasurement
            .Should()
            .NotBeNull();

        capturedMeasurement!.DeviceId.Value
            .Should()
            .Be(deviceId);

        capturedMeasurement.SensorCode
            .Should()
            .Be("A1");

        capturedMeasurement.MeasuredAt
            .Should()
            .Be(measuredAt);

        capturedMeasurement.Temperature
            .Should()
            .Be(25.5);

        capturedMeasurement.Vibration
            .Should()
            .Be(0.35);

        capturedMeasurement.Pressure
            .Should()
            .Be(1013.5);

        capturedMeasurement.Humidity
            .Should()
            .Be(62.5);

        capturedMeasurement.IsFailure
            .Should()
            .BeTrue();

        capturedMeasurement.SensorMeasurementId.Value
            .Should()
            .NotBe(Ulid.Empty);

        result.SensorMeasurementId
            .Should()
            .Be(capturedMeasurement.SensorMeasurementId.Value);
    }

    [Fact]
    public async Task Handle_Should_Create_Measurement_With_Null_Values()
    {
        // Arrange
        var command = new CreateSensorMeasurementCommand(
            Ulid.NewUlid(),
            "A1",
            DateTime.UtcNow,
            null,
            null,
            null,
            null,
            false);

        var handler = new CreateSensorMeasurementCommandHandler(
            _repository.Object,
            _mapper);

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        result.SensorCode
            .Should()
            .Be("A1");

        result.Temperature
            .Should()
            .BeNull();

        result.Vibration
            .Should()
            .BeNull();

        result.Pressure
            .Should()
            .BeNull();

        result.Humidity
            .Should()
            .BeNull();

        result.IsFailure
            .Should()
            .BeFalse();

        _repository.Verify(
            x => x.AddAsync(
                It.IsAny<SensorMeasurement>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Pass_CancellationToken_To_Repository()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource()
            .Token;

        var command = new CreateSensorMeasurementCommand(
            Ulid.NewUlid(),
            "A1",
            DateTime.UtcNow,
            20,
            0.2,
            1000,
            50,
            false);

        var handler = new CreateSensorMeasurementCommandHandler(
            _repository.Object,
            _mapper);

        // Act
        await handler.Handle(
            command,
            cancellationToken);

        // Assert
        _repository.Verify(
            x => x.AddAsync(
                It.IsAny<SensorMeasurement>(),
                cancellationToken),
            Times.Once);
    }
}