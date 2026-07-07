using CollectManagement.Domain.Maintenances;
using CollectManagement.Domain.Maintenances.ObjectValues;
using CollectManagement.Domain.Devices.ValueObjects;

namespace CollectManagement.Application.Interfaces.Repositories.Maintenances;

public interface IMaintenanceRepository : IRepositoryBase<Maintenance>
{
    Task<(IReadOnlyList<Maintenance>, int)> GetPagedListAsync(
        string? search,
        string? sort,
        string? order,
        int page,
        int size,
        string? filter,
        DateTime? fromDate,
        DateTime? toDateExclusive,
        CancellationToken cancellationToken
    );

    Task<Maintenance> GetOneAsync(
        MaintenanceId maintenanceId,
        CancellationToken cancellationToken
    );

    Task UpdateBulkAsync(Maintenance maintenance, CancellationToken cancellationToken);
    
    /// <summary>
    /// Gets the active (not fully completed) maintenance record for an employee identified by RFID.
    /// A maintenance is active if T4Completion is null.
    /// </summary>
    Task<Maintenance?> GetActiveByEmployeeRfidAsync(string rfid, CancellationToken cancellationToken);
    
    /// <summary>
    /// Gets the last maintenance record for an employee (by RFID) assigned to a device (by Matricule).
    /// Used by the MQTT ALARME handler to update T3/T4.
    /// </summary>
    Task<Maintenance?> GetLastByEmployeeRfidAndDeviceMatriculeAsync(string rfid, string matricule, CancellationToken cancellationToken);


    Task<Maintenance?> GetLatestByDeviceIdAsync(DeviceId deviceId, CancellationToken cancellationToken);

    /// <summary>
    /// Checks if there is at least one open (T4Completion is null) maintenance
    /// for the given device on the specified date (based on DateInsertion).
    /// </summary>
    Task<bool> HasOpenMaintenanceForDeviceOnDateAsync(DeviceId deviceId, DateTime date, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the open (T5Confirmation is null) maintenance for the given device on the specified date.
    /// Returns null if none exists.
    /// </summary>
    Task<Maintenance?> GetOpenMaintenanceForDeviceOnDateAsync(DeviceId deviceId, DateTime date, CancellationToken cancellationToken);

    /// <summary>
    /// Gets open maintenances (T5Confirmation is null) for a device with a specific capture code in the description.
    /// </summary>
    Task<IReadOnlyList<Maintenance>> GetOpenMaintenancesByDeviceAndCaptureCodeAsync(
        DeviceId deviceId,
        string captureCode,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets completed maintenances (T3 and T4 both set) with pagination.
    /// </summary>
    Task<(IReadOnlyList<Maintenance>, int)> GetCompletedPagedListAsync(
        string? search,
        int page,
        int size,
        DateTime? fromDate,
        DateTime? toDateExclusive,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Gets all maintenances within a date range (based on DateInsertion), optionally filtered by DeviceId.
    /// </summary>
    Task<IReadOnlyList<Maintenance>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        Ulid? deviceId,
        CancellationToken cancellationToken
    );
    
    Task<IReadOnlyList<Maintenance>> GetByDateRangeAsync1(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken
    );

}
