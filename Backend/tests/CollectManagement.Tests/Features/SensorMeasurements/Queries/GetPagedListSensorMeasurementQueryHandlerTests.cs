using CollectManagement.Application.Features.SensorMeasurements.Mapping;
using CollectManagement.Application.Features.SensorMeasurements.Queries.GetPagedListSensorMeasurement;
using CollectManagement.Application.Interfaces.Repositories.SensorMeasurements;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.SensorMeasurements;
using CollectManagement.Domain.SensorMeasurements.ValueObjects;
using FluentAssertions;
using Mapster;
using MapsterMapper;
using Moq;

namespace CollectManagement.Tests.Features.SensorMeasurements.Queries;

public class GetPagedListSensorMeasurementQueryHandlerTests
{
    private readonly Mock<ISensorMeasurementRepository> _repository;
    private readonly IMapper _mapper;

    public GetPagedListSensorMeasurementQueryHandlerTests()
    {
        _repository = new Mock<ISensorMeasurementRepository>();

        var config = new TypeAdapterConfig();

        new SensorMeasurementMapping().Register(config);

        _mapper = new Mapper(config);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List()
    {
        // Arrange
        _repository
            .Setup(x => x.GetPagedListAsync(
                It.IsAny<Ulid?>(),
                It.IsAny<string?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (
                    new List<SensorMeasurement>()
                        .AsReadOnly()
                        as IReadOnlyList<SensorMeasurement>,
                    0
                ));

        var handler = new GetPagedListSensorMeasurementQueryHandler(
            _repository.Object,
            _mapper);

        var query = new GetPagedListSensorMeasurementQuery(
            null,
            null,
            null,
            null,
            1,
            10);

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        result.Items
            .Should()
            .BeEmpty();

        result.Length
            .Should()
            .Be(0);

        _repository.Verify(
            x => x.GetPagedListAsync(
                null,
                null,
                null,
                null,
                1,
                10,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Mapped_Measurements()
    {
        // Arrange
        var deviceId = Ulid.NewUlid();

        var measurements = new List<SensorMeasurement>
        {
            SensorMeasurement.Create(
                new SensorMeasurementId(Ulid.NewUlid()),
                new DeviceId(deviceId),
                "A1",
                DateTime.UtcNow,
                25.5,
                0.3,
                1013.0,
                60.0,
                false),

            SensorMeasurement.Create(
                new SensorMeasurementId(Ulid.NewUlid()),
                new DeviceId(deviceId),
                "A1",
                DateTime.UtcNow.AddMinutes(1),
                30.0,
                0.5,
                1015.0,
                65.0,
                true)
        };

        _repository
            .Setup(x => x.GetPagedListAsync(
                deviceId,
                "A1",
                null,
                null,
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (
                    measurements.AsReadOnly()
                        as IReadOnlyList<SensorMeasurement>,
                    2
                ));

        var handler = new GetPagedListSensorMeasurementQueryHandler(
            _repository.Object,
            _mapper);

        var query = new GetPagedListSensorMeasurementQuery(
            deviceId,
            "A1",
            null,
            null,
            1,
            10);

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        result.Items
            .Should()
            .HaveCount(2);

        result.Length
            .Should()
            .Be(2);

        result.Items[0].SensorCode
            .Should()
            .Be("A1");

        result.Items[0].Temperature
            .Should()
            .Be(25.5);

        result.Items[0].Vibration
            .Should()
            .Be(0.3);

        result.Items[0].Pressure
            .Should()
            .Be(1013.0);

        result.Items[0].Humidity
            .Should()
            .Be(60.0);

        result.Items[0].IsFailure
            .Should()
            .BeFalse();

        result.Items[1].Temperature
            .Should()
            .Be(30.0);

        result.Items[1].Vibration
            .Should()
            .Be(0.5);

        result.Items[1].Pressure
            .Should()
            .Be(1015.0);

        result.Items[1].Humidity
            .Should()
            .Be(65.0);

        result.Items[1].IsFailure
            .Should()
            .BeTrue();

        _repository.Verify(
            x => x.GetPagedListAsync(
                deviceId,
                "A1",
                null,
                null,
                1,
                10,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Pass_Filters_To_Repository()
    {
        // Arrange
        var deviceId = Ulid.NewUlid();

        var from = new DateTime(
            2026,
            8,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc);

        var to = new DateTime(
            2026,
            8,
            15,
            23,
            59,
            59,
            DateTimeKind.Utc);

        _repository
            .Setup(x => x.GetPagedListAsync(
                deviceId,
                "A1",
                from,
                to,
                2,
                20,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (
                    new List<SensorMeasurement>()
                        .AsReadOnly()
                        as IReadOnlyList<SensorMeasurement>,
                    0
                ));

        var handler = new GetPagedListSensorMeasurementQueryHandler(
            _repository.Object,
            _mapper);

        var query = new GetPagedListSensorMeasurementQuery(
            deviceId,
            "A1",
            from,
            to,
            2,
            20);

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        result.Items
            .Should()
            .BeEmpty();

        result.Length
            .Should()
            .Be(0);

        _repository.Verify(
            x => x.GetPagedListAsync(
                deviceId,
                "A1",
                from,
                to,
                2,
                20,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Correct_Total_Count_When_Page_Contains_Fewer_Items()
    {
        // Arrange
        var deviceId = Ulid.NewUlid();

        var measurements = new List<SensorMeasurement>
        {
            SensorMeasurement.Create(
                new SensorMeasurementId(Ulid.NewUlid()),
                new DeviceId(deviceId),
                "A1",
                DateTime.UtcNow,
                22.0,
                0.2,
                1005.0,
                55.0,
                false)
        };

        _repository
            .Setup(x => x.GetPagedListAsync(
                deviceId,
                "A1",
                null,
                null,
                2,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (
                    measurements.AsReadOnly()
                        as IReadOnlyList<SensorMeasurement>,
                    25
                ));

        var handler = new GetPagedListSensorMeasurementQueryHandler(
            _repository.Object,
            _mapper);

        var query = new GetPagedListSensorMeasurementQuery(
            deviceId,
            "A1",
            null,
            null,
            2,
            10);

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        // Une seule mesure est présente dans cette page
        result.Items
            .Should()
            .HaveCount(1);

        // Mais le nombre total provenant de la base est 25
        result.Length
            .Should()
            .Be(25);
    }

    [Fact]
    public async Task Handle_Should_Pass_CancellationToken_To_Repository()
    {
        // Arrange
        var cancellationTokenSource =
            new CancellationTokenSource();

        var cancellationToken =
            cancellationTokenSource.Token;

        _repository
            .Setup(x => x.GetPagedListAsync(
                It.IsAny<Ulid?>(),
                It.IsAny<string?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                cancellationToken))
            .ReturnsAsync(
                (
                    new List<SensorMeasurement>()
                        .AsReadOnly()
                        as IReadOnlyList<SensorMeasurement>,
                    0
                ));

        var handler = new GetPagedListSensorMeasurementQueryHandler(
            _repository.Object,
            _mapper);

        var query = new GetPagedListSensorMeasurementQuery(
            null,
            null,
            null,
            null,
            1,
            10);

        // Act
        await handler.Handle(
            query,
            cancellationToken);

        // Assert
        _repository.Verify(
            x => x.GetPagedListAsync(
                null,
                null,
                null,
                null,
                1,
                10,
                cancellationToken),
            Times.Once);
    }
}