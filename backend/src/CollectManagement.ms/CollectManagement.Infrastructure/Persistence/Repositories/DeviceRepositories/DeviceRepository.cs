using System.ComponentModel;
using CollectManagement.Application.Interfaces.Repositories.Devices;
using CollectManagement.Domain.Devices;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Infrastructure.Persistence.Context;

namespace CollectManagement.Infrastructure.Persistence.Repositories.DeviceRepositories;

public class DeviceRepository : RepositoryBase<Device>, IDeviceRepository
{
    public DeviceRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<(IReadOnlyList<Device>, int)> GetPagedListAsync(
        string? search,
        string? sort,
        string? order,
        int page,
        int size,
        CancellationToken cancellationToken)
    {
        var where = _dbSet.Where(w =>
            string.IsNullOrWhiteSpace(search) ||
            w.DeviceName.Contains(search) ||
            w.Matricule.Contains(search)
        );

        var orderBy = where
            .OrderByDescending(o => o.DeviceName);

        var prop = TypeDescriptor
            .GetProperties(typeof(Device))
            .Find(sort ?? "DeviceName", true);

        if (prop is not null && order == "asc")
            orderBy = where.OrderBy(o =>
                EF.Property<Device>(o, prop.DisplayName));

        if (prop is not null && order == "desc")
            orderBy = where.OrderByDescending(o =>
                EF.Property<Device>(o, prop.DisplayName));

        var countAsync = await where
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        var readOnlyList = await orderBy
            .AsNoTracking()
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return (readOnlyList, countAsync);
    }

    public Task<Device> GetOneAsync(DeviceId deviceId, CancellationToken cancellationToken)
    {
        return _dbSet.Where(w => w.DeviceId.Equals(deviceId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task UpdateBulkAsync(Device device, CancellationToken cancellationToken)
    {
        await _dbSet.Where(w => w.DeviceId.Equals(device.DeviceId))
            .ExecuteUpdateAsync(
                calls => calls
                    .SetProperty(p => p.DeviceName, device.DeviceName)
                    .SetProperty(p => p.Matricule, device.Matricule)
                    .SetProperty(p => p.NombreCapteur, device.NombreCapteur),
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task UpdateOnlineStatusAsync(
        DeviceId deviceId,
        bool isOnline,
        DateTime lastSeen,
        CancellationToken cancellationToken)
    {
        await _dbSet.Where(w => w.DeviceId.Equals(deviceId))
            .ExecuteUpdateAsync(
                calls => calls
                    .SetProperty(p => p.IsOnline, isOnline)
                    .SetProperty(p => p.LastSeen, lastSeen),
                cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<Device?> GetByMatriculeAsync(string matricule, CancellationToken cancellationToken)
    {
        return _dbSet.Where(w => w.Matricule == matricule)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
