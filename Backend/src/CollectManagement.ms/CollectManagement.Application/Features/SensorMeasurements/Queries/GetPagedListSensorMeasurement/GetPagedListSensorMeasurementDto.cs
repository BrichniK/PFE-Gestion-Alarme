namespace CollectManagement.Application.Features.SensorMeasurements.Queries.GetPagedListSensorMeasurement;

public record GetPagedListSensorMeasurementDto(    Ulid SensorMeasurementId,
    Ulid DeviceId,
    string SensorCode,
    DateTime MeasuredAt,
    double? Temperature,
    double? Vibration,
    double? Pressure,
    double? Humidity,
    bool IsFailure
    );