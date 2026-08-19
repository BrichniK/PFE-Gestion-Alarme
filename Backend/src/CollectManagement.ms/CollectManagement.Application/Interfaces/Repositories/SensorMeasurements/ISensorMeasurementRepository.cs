using CollectManagement.Domain.SensorMeasurements;
using CollectManagement.Domain.SensorMeasurements.ValueObjects;

namespace CollectManagement.Application.Interfaces.Repositories.SensorMeasurements;

public interface ISensorMeasurementRepository : IRepositoryBase<SensorMeasurement>
{
    Task<SensorMeasurement?> GetOneAsync(
        SensorMeasurementId sensorMeasurementId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<SensorMeasurement>> GetForAnalysisAsync(
        Ulid deviceId,
        string? sensorCode,
        CancellationToken cancellationToken);

    Task<(IReadOnlyList<SensorMeasurement>, int)> GetPagedListAsync(
        Ulid? deviceId,
        string? sensorCode,
        DateTime? from,
        DateTime? to,
        int page,
        int size,
        CancellationToken cancellationToken);
}