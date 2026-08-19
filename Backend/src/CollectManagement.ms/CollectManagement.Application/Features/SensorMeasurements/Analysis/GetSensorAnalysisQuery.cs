namespace CollectManagement.Application.Features.SensorMeasurements.Analysis;

public record GetSensorAnalysisQuery(
    Ulid DeviceId,
    string? SensorCode = null
) : IRequest<GetSensorAnalysisResponse>;