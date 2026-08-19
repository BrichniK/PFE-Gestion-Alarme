namespace CollectManagement.Application.Features.SensorMeasurements.Commands.CreateSensorMeasurement;

public record CreateSensorMeasurementCommand(
    Ulid DeviceId,
    string SensorCode,
    DateTime MeasuredAt,
    double? Temperature,
    double? Vibration,
    double? Pressure,
    double? Humidity,
    bool IsFailure = false
) : IRequest<CreateSensorMeasurementResponse>;