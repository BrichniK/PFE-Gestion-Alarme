using CollectManagement.Application.Interfaces.Repositories.SensorMeasurements;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.SensorMeasurements;
using CollectManagement.Domain.SensorMeasurements.ValueObjects;
using CollectManagement.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace CollectManagement.Infrastructure.Persistence.Repositories.SensorMeasurementRepositories;

public class SensorMeasurementRepository
    : RepositoryBase<SensorMeasurement>,
        ISensorMeasurementRepository
{
    public SensorMeasurementRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<SensorMeasurement?> GetOneAsync(
        SensorMeasurementId sensorMeasurementId,
        CancellationToken cancellationToken)
    {
        return await _dbSet
            .Where(x => x.SensorMeasurementId.Equals(sensorMeasurementId))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<SensorMeasurement>> GetForAnalysisAsync(Ulid deviceId, string? sensorCode, CancellationToken cancellationToken)
    {
        var id = new DeviceId(deviceId);

        var query = _dbSet
            .AsNoTracking()
            .Where(x => x.DeviceId.Equals(id));

        if (!string.IsNullOrWhiteSpace(sensorCode))
        {
            query = query.Where(x => x.SensorCode == sensorCode);
        }

        return await query
            .OrderBy(x => x.MeasuredAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

   

    public async Task<(IReadOnlyList<SensorMeasurement>, int)> GetPagedListAsync(
        Ulid? deviceId,
        string? sensorCode,
        DateTime? from,
        DateTime? to,
        int page,
        int size,
        CancellationToken cancellationToken)
    {
        var query = _dbSet.AsQueryable();

        if (deviceId.HasValue)
        {
            var id = new DeviceId(deviceId.Value);

            query = query.Where(x => x.DeviceId.Equals(id));
        }

        if (!string.IsNullOrWhiteSpace(sensorCode))
        {
            query = query.Where(x =>
                x.SensorCode.Contains(sensorCode));
        }

        if (from.HasValue)
        {
            query = query.Where(x =>
                x.MeasuredAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x =>
                x.MeasuredAt <= to.Value);
        }

        var count = await query
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        var measurements = await query
            .OrderByDescending(x => x.MeasuredAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return (measurements, count);
    }
}