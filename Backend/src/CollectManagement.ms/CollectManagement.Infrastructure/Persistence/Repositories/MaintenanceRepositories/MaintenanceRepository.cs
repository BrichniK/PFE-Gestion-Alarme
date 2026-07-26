using System.ComponentModel;
using CollectManagement.Application.Interfaces.Repositories.Maintenances;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Maintenances;
using CollectManagement.Domain.Maintenances.ObjectValues;
using CollectManagement.Infrastructure.Persistence.Context;

namespace CollectManagement.Infrastructure.Persistence.Repositories.MaintenanceRepositories;

public class MaintenanceRepository : RepositoryBase<Maintenance>, IMaintenanceRepository
{
    public MaintenanceRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<(IReadOnlyList<Maintenance>, int)> GetPagedListAsync(
        string? search,
        string? sort,
        string? order,
        int page,
        int size,
        string? filter,
        DateTime? fromDate,
        DateTime? toDateExclusive,
        CancellationToken cancellationToken)
    {
        var where = _dbSet
            .Include(i => i.Device)
            .Include(i => i.Employee)
            .Where(w =>
                (string.IsNullOrWhiteSpace(search) ||
                w.Description.Contains(search) ||
                w.Device.DeviceName.Contains(search) ||
                w.Employee.Nom.Contains(search) ||
                w.Employee.Prenom.Contains(search))
            &&
            (filter == null || filter == "all" ||
                (filter == "affecte" && w.T3Arrival == null && w.T4Completion == null && w.T5Confirmation == null) ||
                (filter == "diagnostique" && w.T3Arrival != null && w.T4Completion == null && w.T5Confirmation == null) ||
                (filter == "reparation" && w.T3Arrival != null && w.T4Completion != null && w.T5Confirmation == null) ||
                (filter == "done" && w.T3Arrival != null && w.T4Completion != null && w.T5Confirmation != null))
            &&
            (fromDate == null || (w.T1Alerte ?? w.DateInsertion) >= fromDate)
            &&
            (toDateExclusive == null || (w.T1Alerte ?? w.DateInsertion) < toDateExclusive)
            );

        var orderBy = where
            .OrderByDescending(o => o.T1Alerte);

        var prop = TypeDescriptor
            .GetProperties(typeof(Maintenance))
            .Find(sort ?? "T1Alerte", true);

        if (prop is not null && order == "asc")
            orderBy = where.OrderBy(o =>
                EF.Property<Maintenance>(o, prop.DisplayName));

        if (prop is not null && order == "desc")
            orderBy = where.OrderByDescending(o =>
                EF.Property<Maintenance>(o, prop.DisplayName));

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

    public Task<Maintenance> GetOneAsync(MaintenanceId maintenanceId, CancellationToken cancellationToken)
    {
        return _dbSet
            .Include(i => i.Device)
            .Include(i => i.Employee)
            .Where(w => w.MaintenanceId.Equals(maintenanceId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task UpdateBulkAsync(Maintenance maintenance, CancellationToken cancellationToken)
    {
        await _dbSet.Where(w => w.MaintenanceId.Equals(maintenance.MaintenanceId))
            .ExecuteUpdateAsync(
                calls => calls
                    .SetProperty(p => p.DeviceId, maintenance.DeviceId)
                    .SetProperty(p => p.EmployeeId, maintenance.EmployeeId)
                    .SetProperty(p => p.T1Alerte, maintenance.T1Alerte)
                    .SetProperty(p => p.T2Assignment, maintenance.T2Assignment)
                    .SetProperty(p => p.T3Arrival, maintenance.T3Arrival)
                    .SetProperty(p => p.T4Completion, maintenance.T4Completion)
                    .SetProperty(p => p.T5Confirmation, maintenance.T5Confirmation)
                    .SetProperty(p => p.T6NextAlert, maintenance.T6NextAlert)
                    .SetProperty(p => p.Description, maintenance.Description),
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Maintenance?> GetActiveByEmployeeRfidAsync(string rfid, CancellationToken cancellationToken)
    {
        return await _dbSet
            .Include(i => i.Device)
            .Include(i => i.Employee)
            .Where(w => w.Employee.Rfid == rfid && w.T5Confirmation == null)
            .OrderByDescending(o => o.T1Alerte)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Maintenance?> GetLastByEmployeeRfidAndDeviceMatriculeAsync(string rfid, string matricule, CancellationToken cancellationToken)
    {
        return await _dbSet
            .Include(i => i.Device)
            .Include(i => i.Employee)
            .Where(w => w.Employee.Rfid == rfid && w.Device.Matricule == matricule)
            .OrderByDescending(o => o.DateInsertion)
            .ThenByDescending(o => o.T1Alerte)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }


    public async Task<Maintenance?> GetLatestByDeviceIdAsync(DeviceId deviceId, CancellationToken cancellationToken)
    {
        return await _dbSet
            .Include(i => i.Device)
            .Include(i => i.Employee)
            .Where(w => w.DeviceId == deviceId)
            .OrderByDescending(o => o.DateInsertion)
            .ThenByDescending(o => o.T1Alerte)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> HasOpenMaintenanceForDeviceOnDateAsync(DeviceId deviceId, DateTime date, CancellationToken cancellationToken)
    {
        var start = date.Date;
        var end = start.AddDays(1);

        return await _dbSet
            .AnyAsync(w =>
                    w.DeviceId == deviceId &&
                    w.T5Confirmation == null &&
                    w.DateInsertion >= start &&
                    w.DateInsertion < end,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Maintenance?> GetOpenMaintenanceForDeviceOnDateAsync(DeviceId deviceId, DateTime date, CancellationToken cancellationToken)
    {
        var start = date.Date;
        var end = start.AddDays(1);

        return await _dbSet
            .Include(i => i.Device)
            .Include(i => i.Employee)
            .Where(w =>
                w.DeviceId == deviceId &&
                w.T5Confirmation == null &&
                w.DateInsertion >= start &&
                w.DateInsertion < end)
            .OrderByDescending(o => o.DateInsertion)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<(IReadOnlyList<Maintenance>, int)> GetCompletedPagedListAsync(
        string? search,
        int page,
        int size,
        DateTime? fromDate,
        DateTime? toDateExclusive,
        CancellationToken cancellationToken)
    {
        var where = _dbSet
            .Include(i => i.Device)
            .Include(i => i.Employee)
            .Where(w => w.T3Arrival.HasValue && w.T4Completion.HasValue && w.T5Confirmation.HasValue)
            .Where(w =>
                string.IsNullOrWhiteSpace(search) ||
                w.Device.DeviceName.Contains(search) ||
                w.Employee.Nom.Contains(search) ||
                w.Employee.Prenom.Contains(search)
            )
            .Where(w => fromDate == null || (w.T1Alerte ?? w.DateInsertion) >= fromDate)
            .Where(w => toDateExclusive == null || (w.T1Alerte ?? w.DateInsertion) < toDateExclusive);

        var countAsync = await where
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        var readOnlyList = await where
            .OrderByDescending(o => o.T4Completion)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return (readOnlyList, countAsync);

    }

    public async Task<IReadOnlyList<Maintenance>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        Ulid? deviceId,
        CancellationToken cancellationToken)
    {
        var query = _dbSet
            .Where(w => w.DateInsertion >= startDate && w.DateInsertion < endDate);

        if (deviceId.HasValue)
        {
            var did = new DeviceId(deviceId.Value);
            query = query.Where(w => w.DeviceId == did);
        }

        return await query
            .OrderByDescending(o => o.DateInsertion)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
    
    public async Task<IReadOnlyList<Maintenance>> GetByDateRangeAsync1(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken)
    {
        return await _dbSet
            .Where(w =>
                (w.T1Alerte.HasValue && w.T1Alerte.Value >= startDate && w.T1Alerte.Value < endDate) ||
                (w.T2Assignment.HasValue && w.T2Assignment.Value >= startDate && w.T2Assignment.Value < endDate) ||
                (w.T3Arrival.HasValue && w.T3Arrival.Value >= startDate && w.T3Arrival.Value < endDate) ||
                (w.T4Completion.HasValue && w.T4Completion.Value >= startDate && w.T4Completion.Value < endDate) ||
                (w.T5Confirmation.HasValue && w.T5Confirmation.Value >= startDate && w.T5Confirmation.Value < endDate) ||
                (w.T6NextAlert.HasValue && w.T6NextAlert.Value >= startDate && w.T6NextAlert.Value < endDate) ||
                (w.DateInsertion.HasValue && w.DateInsertion.Value >= startDate && w.DateInsertion.Value < endDate))
            .OrderByDescending(o => o.T1Alerte ?? o.DateInsertion)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Maintenance>> GetOpenMaintenancesByDeviceAndCaptureCodeAsync(
        DeviceId deviceId,
        string captureCode,
        CancellationToken cancellationToken)
    {
        var descriptionFilter = $"CAPTURE_CODE:{captureCode}";
        return await _dbSet
            .Include(m => m.Employee)
            .Include(m => m.Device)
            .Where(m => m.DeviceId == deviceId
                     && !m.T5Confirmation.HasValue
                     && m.Description == descriptionFilter)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
