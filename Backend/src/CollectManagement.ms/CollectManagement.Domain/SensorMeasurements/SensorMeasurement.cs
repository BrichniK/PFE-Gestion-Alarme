using CollectManagement.Domain.Common;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.SensorMeasurements.ValueObjects;

namespace CollectManagement.Domain.SensorMeasurements;

public class SensorMeasurement : AuditableEntity
{
    public SensorMeasurementId SensorMeasurementId { get; private set; }

    public DeviceId DeviceId { get; private set; }

    public string SensorCode { get; private set; }

    public DateTime MeasuredAt { get; private set; }

    public double? Temperature { get; private set; }

    public double? Vibration { get; private set; }

    public double? Pressure { get; private set; }

    public double? Humidity { get; private set; }

    public bool IsFailure { get; private set; }

    private SensorMeasurement(
        SensorMeasurementId sensorMeasurementId,
        DeviceId deviceId,
        string sensorCode,
        DateTime measuredAt,
        double? temperature,
        double? vibration,
        double? pressure,
        double? humidity,
        bool isFailure)
    {
        SensorMeasurementId = sensorMeasurementId;
        DeviceId = deviceId;
        SensorCode = sensorCode;
        MeasuredAt = measuredAt;
        Temperature = temperature;
        Vibration = vibration;
        Pressure = pressure;
        Humidity = humidity;
        IsFailure = isFailure;
    }

    public static SensorMeasurement Create(
        SensorMeasurementId sensorMeasurementId,
        DeviceId deviceId,
        string sensorCode,
        DateTime measuredAt,
        double? temperature,
        double? vibration,
        double? pressure,
        double? humidity,
        bool isFailure = false)
    {
        return new SensorMeasurement(
            sensorMeasurementId,
            deviceId,
            sensorCode,
            measuredAt,
            temperature,
            vibration,
            pressure,
            humidity,
            isFailure);
    }

    public void Update(
        DeviceId deviceId,
        string sensorCode,
        DateTime measuredAt,
        double? temperature,
        double? vibration,
        double? pressure,
        double? humidity,
        bool isFailure)
    {
        DeviceId = deviceId;
        SensorCode = sensorCode;
        MeasuredAt = measuredAt;
        Temperature = temperature;
        Vibration = vibration;
        Pressure = pressure;
        Humidity = humidity;
        IsFailure = isFailure;
    }

#pragma warning disable CS8618
    private SensorMeasurement()
    {
    }
#pragma warning restore CS8618
}