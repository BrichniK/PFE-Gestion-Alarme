namespace CollectManagement.Application.Features.SensorMeasurements.Queries.GetPagedListSensorMeasurement;

public record GetPagedListSensorMeasurementQuery(    Ulid? DeviceId,
    string? SensorCode,
    DateTime? From,
    DateTime? To,
    int Page,
    int Size
) : IRequest<GetPagedListSensorMeasurementResponse>;