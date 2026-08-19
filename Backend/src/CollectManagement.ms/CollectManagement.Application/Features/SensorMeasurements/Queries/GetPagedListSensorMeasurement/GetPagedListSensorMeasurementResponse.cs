namespace CollectManagement.Application.Features.SensorMeasurements.Queries.GetPagedListSensorMeasurement;

public record GetPagedListSensorMeasurementResponse(    IReadOnlyList<GetPagedListSensorMeasurementDto> Items,
    int Length);