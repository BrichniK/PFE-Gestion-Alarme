using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.SensorMeasurements;
using CollectManagement.Domain.SensorMeasurements.ValueObjects;
using FluentAssertions;

namespace CollectManagement.Tests.Features.Domain.SensorMeasurements;

public class SensorMeasurementTests
{

    [Fact]
    public void Create_Should_Create_SensorMeasurement()
    {

        var id       = new SensorMeasurementId(Ulid.NewUlid());
        var deviceId = new DeviceId(Ulid.NewUlid());
        var date     = DateTime.UtcNow;


        var sm = SensorMeasurement.Create(
            id,
            deviceId,
            "CAPTEUR-01",
            date,
            25.5,
            0.3,
            1013.0,
            60.0
        );


        sm.Should().NotBeNull();

        sm.SensorMeasurementId.Should().Be(id);

        sm.DeviceId.Should().Be(deviceId);

        sm.SensorCode.Should().Be("CAPTEUR-01");

        sm.MeasuredAt.Should().Be(date);

        sm.Temperature.Should().Be(25.5);

        sm.Vibration.Should().Be(0.3);

        sm.Pressure.Should().Be(1013.0);

        sm.Humidity.Should().Be(60.0);

        sm.IsFailure.Should().BeFalse();
    }


    [Fact]
    public void Create_With_IsFailure_True_Should_Set_IsFailure()
    {

        var sm = SensorMeasurement.Create(
            new SensorMeasurementId(Ulid.NewUlid()),
            new DeviceId(Ulid.NewUlid()),
            "CAPTEUR-01",
            DateTime.UtcNow,
            null, null, null, null,
            isFailure: true
        );


        sm.IsFailure.Should().BeTrue();
    }


    [Fact]
    public void Create_With_Null_Measures_Should_Be_Allowed()
    {

        var sm = SensorMeasurement.Create(
            new SensorMeasurementId(Ulid.NewUlid()),
            new DeviceId(Ulid.NewUlid()),
            "CAPTEUR-02",
            DateTime.UtcNow,
            null, null, null, null
        );


        sm.Temperature.Should().BeNull();

        sm.Vibration.Should().BeNull();

        sm.Pressure.Should().BeNull();

        sm.Humidity.Should().BeNull();
    }


    [Fact]
    public void Update_Should_Modify_SensorMeasurement()
    {

        var sm = SensorMeasurement.Create(
            new SensorMeasurementId(Ulid.NewUlid()),
            new DeviceId(Ulid.NewUlid()),
            "OLD-CODE",
            DateTime.UtcNow,
            20.0, 0.1, 1000.0, 50.0
        );

        var newDeviceId = new DeviceId(Ulid.NewUlid());
        var newDate     = DateTime.UtcNow.AddHours(1);


        sm.Update(
            newDeviceId,
            "NEW-CODE",
            newDate,
            30.0, 0.5, 1020.0, 70.0,
            true
        );


        sm.DeviceId.Should().Be(newDeviceId);

        sm.SensorCode.Should().Be("NEW-CODE");

        sm.MeasuredAt.Should().Be(newDate);

        sm.Temperature.Should().Be(30.0);

        sm.Vibration.Should().Be(0.5);

        sm.Pressure.Should().Be(1020.0);

        sm.Humidity.Should().Be(70.0);

        sm.IsFailure.Should().BeTrue();
    }
}
