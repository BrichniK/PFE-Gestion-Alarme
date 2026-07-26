using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Maintenances;

namespace CollectManagement.Application.Interfaces.Repositories.Maintenances;

public interface IMaintenanceCaptureHistoryRepository : IRepositoryBase<MaintenanceCaptureHistory>
{
    Task<(IReadOnlyList<MaintenanceCaptureHistory>, int)> GetPagedByDeviceIdAsync(
        DeviceId deviceId,
        int page,
        int size,
        CancellationToken cancellationToken);
}
