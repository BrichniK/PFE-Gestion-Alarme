using System.ComponentModel;
using CollectManagement.Application.Interfaces.Repositories.Alertes;
using CollectManagement.Domain.Alertes;
using CollectManagement.Domain.Alertes.ValueObjects;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Types.ValueObjects;
using CollectManagement.Infrastructure.Persistence.Context;
using System.Globalization;

namespace CollectManagement.Infrastructure.Persistence.Repositories.AlerteRepositories;

public class AlerteRepository : RepositoryBase<Alerte>, IAlerteRepository
{
    public AlerteRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<(IReadOnlyList<Alerte>, int)> GetPagedListAsync(
        string? search,
        string? sort,
        string? order,
        int page,
        int size,
        CancellationToken cancellationToken)
    {
        var where = _dbSet.Where(w =>
            string.IsNullOrWhiteSpace(search) ||
            w.Dispositif.DeviceName.Contains(search) ||
            w.Dispositif.Matricule.Contains(search)
        ).Include(w=>w.Dispositif);

        var orderBy = where
            .OrderByDescending(o => o.Date);

        var prop = TypeDescriptor
            .GetProperties(typeof(Alerte))
            .Find(sort ?? "Date", true);

        if (prop is not null && order == "asc")
            orderBy = where.OrderBy(o =>
                EF.Property<Alerte>(o, prop.DisplayName));

        if (prop is not null && order == "desc")
            orderBy = where.OrderByDescending(o =>
                EF.Property<Alerte>(o, prop.DisplayName));

        var countAsync = await where
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        var readOnlyList = await orderBy
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return (readOnlyList, countAsync);
    }

    public Task<Alerte> GetOneAsync(AlerteId alerteId, CancellationToken cancellationToken)
    {
        return _dbSet
            .Include(i => i.Type)
            .Include(i => i.Dispositif)
            .Where(w => w.AlerteId.Equals(alerteId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task UpdateBulkAsync(Alerte alerte, CancellationToken cancellationToken)
    {
        await _dbSet.Where(w => w.AlerteId.Equals(alerte.AlerteId))
            .ExecuteUpdateAsync(
                calls => calls
                    .SetProperty(p => p.Date, alerte.Date)
                    .SetProperty(p => p.DispositifId, alerte.DispositifId)
                    .SetProperty(p => p.TypeId, alerte.TypeId)
                    .SetProperty(p => p.Traiter, alerte.Traiter),
                cancellationToken)
            .ConfigureAwait(false);
    }


    public async Task<Alerte?> GetLatestCaptureAlertByDeviceAndCodeAsync(
        DeviceId deviceId,
        string code,
        CancellationToken cancellationToken)
    {
        return await _dbSet
            .Include(i => i.Dispositif)
            .Include(i => i.Type)
            .Where(w => w.DispositifId == deviceId && w.Type.Code == code)
            .OrderByDescending(o => o.Date)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyDictionary<string, Alerte>> GetLatestUnprocessedCaptureAlertsByDeviceAsync(
        DeviceId deviceId,
        CancellationToken cancellationToken)
    {
        var alerts = await _dbSet
            .Include(i => i.Type)
            .Where(w => w.DispositifId == deviceId && !w.Traiter && w.Type.Code.StartsWith("A"))
            .OrderByDescending(o => o.Date)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return alerts
            .Where(alert => TryGetCaptureIndex(alert.Type?.Code, out _))
            .GroupBy(alert => alert.Type.Code, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
    }

    public async Task<Alerte?> GetLatestProcessedCaptureAlertByDeviceBeforeAsync(
        DeviceId deviceId,
        DateTime? before,
        CancellationToken cancellationToken)
    {
        var query = _dbSet
            .Include(i => i.Type)
            .Where(w => w.DispositifId == deviceId && w.Traiter && w.Type.Code.StartsWith("A"));

        if (before.HasValue)
        {
            var windowStart = before.Value.AddHours(-6);
            query = query.Where(w => w.Date >= windowStart && w.Date <= before.Value);
        }

        return await query
            .OrderByDescending(o => o.Date)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    private static bool TryGetCaptureIndex(string? code, out int captureIndex)
    {
        captureIndex = 0;
        if (string.IsNullOrWhiteSpace(code) || code.Length < 2 || !code.StartsWith("A", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return int.TryParse(code[1..], NumberStyles.None, CultureInfo.InvariantCulture, out captureIndex)
               && captureIndex > 0;
    }

    public async Task<Alerte?> GetByDeviceIdAndDateAsync(
        DeviceId deviceId,
        DateTime date,
        CancellationToken cancellationToken)
    {
        return await _dbSet
            .Where(w => w.DispositifId.Equals(deviceId) && w.Date.HasValue && w.Date.Value.Date == date.Date)
            .Include(w => w.Type)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> ExistsByDeviceDateAndTypeAsync(
        DeviceId deviceId,
        DateTime date,
        TypeId typeId,
        CancellationToken cancellationToken)
    {
        return await _dbSet
            .AnyAsync(a => a.DispositifId.Equals(deviceId)
                        && a.TypeId.Equals(typeId)
                        && a.Date.HasValue
                        && a.Date.Value == date,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Alerte?> GetLatestUnprocessedByDeviceIdAsync(
        DeviceId deviceId,
        CancellationToken cancellationToken)
    {
        return await _dbSet
            .Include(i => i.Type)
            .Where(w => w.DispositifId == deviceId && !w.Traiter)
            .OrderByDescending(o => o.Date)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<int> DeleteUnprocessedByDeviceAndTypeCodeAsync(
        DeviceId deviceId,
        string typeCode,
        CancellationToken cancellationToken)
    {
        return await _dbSet
            .Where(a => a.DispositifId == deviceId
                     && !a.Traiter
                     && a.Type.Code == typeCode)
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Alerte>> GetRecentByDeviceIdAsync(
        DeviceId deviceId,
        int count,
        CancellationToken cancellationToken)
    {
        if (count <= 0)
        {
            return Array.Empty<Alerte>();
        }

        return await _dbSet
            .AsNoTracking()
            .Include(a => a.Type)
            .Where(a => a.DispositifId == deviceId)
            .OrderByDescending(a => a.Date)
            .Take(count)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
