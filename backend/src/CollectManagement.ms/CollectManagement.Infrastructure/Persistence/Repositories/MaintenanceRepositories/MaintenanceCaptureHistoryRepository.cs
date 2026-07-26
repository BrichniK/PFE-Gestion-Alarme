using CollectManagement.Application.Interfaces.Repositories.Maintenances;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Maintenances;
using CollectManagement.Infrastructure.Persistence.Context;

namespace CollectManagement.Infrastructure.Persistence.Repositories.MaintenanceRepositories;

public class MaintenanceCaptureHistoryRepository : RepositoryBase<MaintenanceCaptureHistory>, IMaintenanceCaptureHistoryRepository
{
    public MaintenanceCaptureHistoryRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<(IReadOnlyList<MaintenanceCaptureHistory>, int)> GetPagedByDeviceIdAsync(
        DeviceId deviceId,
        int page,
        int size,
        CancellationToken cancellationToken)
    {
        var where = _dbSet
            .Include(i => i.Device)
            .Include(i => i.Employee)
            .Include(i => i.Maintenance)
            .Where(w => w.DeviceId == deviceId);

        var count = await where
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        var list = await where
            .OrderByDescending(o => o.CapturedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return (list, count);
    }
}
