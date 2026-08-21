using System.ComponentModel;
using CollectManagement.Application.Features.Alertes.Queries.GetEmployeesByPlanning;
using CollectManagement.Application.Interfaces.Repositories.Plannings;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Employess;
using CollectManagement.Domain.Employess.ObjectValues;
using CollectManagement.Domain.Groupes;
using CollectManagement.Domain.Plannings;
using CollectManagement.Domain.Plannings.ValueObjects;
using CollectManagement.Infrastructure.Persistence.Context;

namespace CollectManagement.Infrastructure.Persistence.Repositories.PlanningRepositories;

public class PlanningRepository : RepositoryBase<Planning>, IPlanningRepository
{
    private readonly ApplicationDbContext _context;

    public PlanningRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _context = dbContext;
    }

    public async Task<(IReadOnlyList<Planning>, int)> GetPagedListAsync(
        string? search,
        string? sort,
        string? order,
        int page,
        int size,
        CancellationToken cancellationToken)
    {
        var where = _dbSet
            .Include(i => i.PlanningGroupes)
            .ThenInclude(i => i.Groupe)
            .Include(i => i.PlanningDevices)
            .ThenInclude(i => i.Device)
            .Include(i => i.PlanningShifts)
            .ThenInclude(i => i.Shift)
            .Include(i => i.PlanningEmployees)
            .ThenInclude(i => i.Employee)
            .Where(w =>
                string.IsNullOrWhiteSpace(search) ||
                w.PlanningGroupes.Any(pg =>
                    pg.Groupe.Nom.Contains(search)) ||
                w.PlanningDevices.Any(pd =>
                    pd.Device.DeviceName.Contains(search)) ||
                w.PlanningShifts.Any(ps =>
                    ps.Shift.Label.Contains(search))
            );

        IOrderedQueryable<Planning> orderBy;

        if (string.Equals(sort, "Date", StringComparison.OrdinalIgnoreCase))
        {
            orderBy = string.Equals(order, "asc", StringComparison.OrdinalIgnoreCase)
                ? where.OrderBy(o => o.Date)
                : where.OrderByDescending(o => o.Date);
        }
        else
        {
            // Tri par défaut
            orderBy = where.OrderByDescending(o => o.Date);
        }

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

    public Task<Planning> GetOneAsync(PlanningId planningId, CancellationToken cancellationToken)
    {
        return _dbSet
            .Include(i => i.PlanningGroupes)
            .ThenInclude(i => i.Groupe)
            .Include(i => i.PlanningDevices)
            .ThenInclude(i => i.Device)
            .Include(i => i.PlanningShifts)
            .ThenInclude(i => i.Shift)
            .Include(i => i.PlanningEmployees)
            .ThenInclude(i => i.Employee)
            .Where(w => w.PlanningId.Equals(planningId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<EmployeePlanningDto>> GetEmployeesByDateAndDeviceAsync(
        DateTime date,
        DeviceId deviceId,
        CancellationToken cancellationToken)
    {
        var groupes = await _dbSet
            .Where(p => p.Date.Date == date.Date)
            .Where(p => p.PlanningDevices.Any(pd => pd.DeviceId.Equals(deviceId)))
            .SelectMany(p => p.PlanningGroupes)
            .Select(pg => pg.Groupe)
            .Distinct()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var employeeIds = groupes
            .SelectMany(g => g.EmployeeIds ?? new List<Ulid>())
            .Distinct()
            .Select(id => new EmployeeId(id))
            .ToList();

        if (employeeIds.Count == 0)
            return new List<EmployeePlanningDto>();

        var employees = await _context.Set<Employee>()
            .Where(e => employeeIds.Contains(e.EmployeeId))
            .Select(e => new EmployeePlanningDto(
                e.EmployeeId.Value,
                e.Nom,
                e.Prenom,
                e.Phone,
                e.Email))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return employees;
    }

    public async Task<List<GroupeWithEmployeesDto>> GetGroupesWithEmployeesByDateAndDeviceAsync(
        DateTime date,
        DeviceId deviceId,
        CancellationToken cancellationToken)
    {
        var plannings = await _dbSet
            .Where(p => p.Date.Date == date.Date)
            .Where(p => p.PlanningDevices.Any(pd => pd.DeviceId.Equals(deviceId)))
            .Include(p => p.PlanningGroupes)
                .ThenInclude(pg => pg.Groupe)
            .Include(p => p.PlanningShifts)
                .ThenInclude(ps => ps.Shift)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var allEmployeeIds = plannings
            .SelectMany(p => p.PlanningGroupes)
            .SelectMany(pg => pg.Groupe.EmployeeIds ?? new List<Ulid>())
            .Distinct()
            .Select(id => new EmployeeId(id))
            .ToList();

        var employeesMap = new Dictionary<Ulid, Employee>();
        if (allEmployeeIds.Count > 0)
        {
            var employees = await _context.Set<Employee>()
                .Where(e => allEmployeeIds.Contains(e.EmployeeId))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            employeesMap = employees.ToDictionary(e => e.EmployeeId.Value);
        }

        var result = new List<GroupeWithEmployeesDto>();

        foreach (var planning in plannings)
        {
            var shiftLabel = string.Join(", ", planning.PlanningShifts.Select(ps => ps.Shift.Label));
            var shiftStart = planning.PlanningShifts.FirstOrDefault()?.Shift.StartTime.ToString("HH:mm") ?? "";
            var shiftEnd = planning.PlanningShifts.FirstOrDefault()?.Shift.EndTime.ToString("HH:mm") ?? "";

            foreach (var pg in planning.PlanningGroupes)
            {
                var groupe = pg.Groupe;
                var empDtos = (groupe.EmployeeIds ?? new List<Ulid>())
                    .Where(id => employeesMap.ContainsKey(id))
                    .Select(id => employeesMap[id])
                    .Select(e => new EmployeePlanningDto(e.EmployeeId.Value, e.Nom, e.Prenom, e.Phone, e.Email))
                    .ToList();

                result.Add(new GroupeWithEmployeesDto(
                    groupe.Nom,
                    shiftLabel,
                    shiftStart,
                    shiftEnd,
                    empDtos
                ));
            }
        }

        return result;
    }
}
