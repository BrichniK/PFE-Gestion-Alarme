using CollectManagement.Application.Features.SensorMeasurements.Commands.CreateSensorMeasurement;
using CollectManagement.Application.Features.SensorMeasurements.Mapping;
using CollectManagement.Application.Features.SensorMeasurements.Queries.GetPagedListSensorMeasurement;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.SensorMeasurements;
using CollectManagement.Domain.SensorMeasurements.ValueObjects;
using FluentAssertions;
using Mapster;

namespace CollectManagement.Tests.Features.SensorMeasurements.Mapping;

public class SensorMeasurementMappingTests
{

    private readonly TypeAdapterConfig _config;


    public SensorMeasurementMappingTests()
    {
        _config = new TypeAdapterConfig();
        new SensorMeasurementMapping().Register(_config);
    }


    [Fact]
    public void Should_Map_SensorMeasurement_To_CreateSensorMeasurementResponse()
    {

        var id       = new SensorMeasurementId(Ulid.NewUlid());
        var deviceId = new DeviceId(Ulid.NewUlid());
        var date     = DateTime.UtcNow;

        var sm = SensorMeasurement.Create(
            id, deviceId,
            "CAPTEUR-01", date,
            25.5, 0.3, 1013.0, 60.0
        );


        var result = sm.Adapt<CreateSensorMeasurementResponse>(_config);


        result.Should().NotBeNull();

        result.SensorMeasurementId.Should().Be(id.Value);

        result.DeviceId.Should().Be(deviceId.Value);

        result.SensorCode.Should().Be("CAPTEUR-01");

        result.MeasuredAt.Should().Be(date);

        result.Temperature.Should().Be(25.5);

        result.Vibration.Should().Be(0.3);

        result.Pressure.Should().Be(1013.0);

        result.Humidity.Should().Be(60.0);

        result.IsFailure.Should().BeFalse();
    }


    [Fact]
    public void Should_Map_SensorMeasurement_To_GetPagedListSensorMeasurementDto()
    {

        var id       = new SensorMeasurementId(Ulid.NewUlid());
        var deviceId = new DeviceId(Ulid.NewUlid());

        var sm = SensorMeasurement.Create(
            id, deviceId,
            "CAPTEUR-02", DateTime.UtcNow,
            null, null, null, null,
            isFailure: true
        );


        var result = sm.Adapt<GetPagedListSensorMeasurementDto>(_config);


        result.Should().NotBeNull();

        result.SensorMeasurementId.Should().Be(id.Value);

        result.DeviceId.Should().Be(deviceId.Value);

        result.IsFailure.Should().BeTrue();

        result.Temperature.Should().BeNull();
    }
}
