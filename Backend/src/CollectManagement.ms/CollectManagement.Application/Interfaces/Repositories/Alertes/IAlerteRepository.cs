using CollectManagement.Domain.Alertes;
using CollectManagement.Domain.Alertes.ValueObjects;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Types.ValueObjects;

namespace CollectManagement.Application.Interfaces.Repositories.Alertes;

public interface IAlerteRepository : IRepositoryBase<Alerte>
{
    Task<(IReadOnlyList<Alerte>, int)> GetPagedListAsync(
        string? search,
        string? sort,
        string? order,
        int page,
        int size,
        CancellationToken cancellationToken
    );

    Task<Alerte> GetOneAsync(
        AlerteId alerteId,
        CancellationToken cancellationToken
    );

    Task UpdateBulkAsync(
        Alerte alerte,
        CancellationToken cancellationToken);

    Task<Alerte?> GetLatestCaptureAlertByDeviceAndCodeAsync(
        DeviceId deviceId,
        string code,
        CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<string, Alerte>>
        GetLatestUnprocessedCaptureAlertsByDeviceAsync(
            DeviceId deviceId,
            CancellationToken cancellationToken);

    Task<Alerte?> GetLatestProcessedCaptureAlertByDeviceBeforeAsync(
        DeviceId deviceId,
        DateTime? before,
        CancellationToken cancellationToken);

    Task<Alerte?> GetByDeviceIdAndDateAsync(
        DeviceId deviceId,
        DateTime date,
        CancellationToken cancellationToken
    );

    Task<bool> ExistsByDeviceDateAndTypeAsync(
        DeviceId deviceId,
        DateTime date,
        TypeId typeId,
        CancellationToken cancellationToken);

    Task<Alerte?> GetLatestUnprocessedByDeviceIdAsync(
        DeviceId deviceId,
        CancellationToken cancellationToken);

    Task<int> DeleteUnprocessedByDeviceAndTypeCodeAsync(
        DeviceId deviceId,
        string typeCode,
        CancellationToken cancellationToken);

    // Nouvelle méthode utilisée par l'analyse IA
    Task<IReadOnlyList<Alerte>> GetRecentByDeviceIdAsync(
        DeviceId deviceId,
        int count,
        CancellationToken cancellationToken);
}