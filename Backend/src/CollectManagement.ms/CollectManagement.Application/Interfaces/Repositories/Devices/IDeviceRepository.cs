using CollectManagement.Domain.Devices;
using CollectManagement.Domain.Devices.ValueObjects;

namespace CollectManagement.Application.Interfaces.Repositories.Devices;

public interface IDeviceRepository : IRepositoryBase<Device>
{
    Task<(IReadOnlyList<Device>, int)> GetPagedListAsync(
        string? search,
        string? sort,
        string? order,
        int page,
        int size,
        CancellationToken cancellationToken
    );

    Task<Device> GetOneAsync(
        DeviceId deviceId,
        CancellationToken cancellationToken
    );

    Task UpdateBulkAsync(Device device, CancellationToken cancellationToken);

    Task UpdateOnlineStatusAsync(
        DeviceId deviceId,
        bool isOnline,
        DateTime lastSeen,
        CancellationToken cancellationToken);

    Task<Device?> GetByMatriculeAsync(string matricule, CancellationToken cancellationToken);
}
