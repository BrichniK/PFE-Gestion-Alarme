using CollectManagement.Application.Features.Alertes.Queries.GetEmployeesByPlanning;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Plannings;
using CollectManagement.Domain.Plannings.ValueObjects;

namespace CollectManagement.Application.Interfaces.Repositories.Plannings;

public interface IPlanningRepository : IRepositoryBase<Planning>
{
    Task<(IReadOnlyList<Planning>, int)> GetPagedListAsync(
        string? search,
        string? sort,
        string? order,
        int page,
        int size,
        CancellationToken cancellationToken
    );

    Task<Planning> GetOneAsync(
        PlanningId planningId,
        CancellationToken cancellationToken
    );

    Task<List<EmployeePlanningDto>> GetEmployeesByDateAndDeviceAsync(
        DateTime date,
        DeviceId deviceId,
        CancellationToken cancellationToken
    );

    Task<List<GroupeWithEmployeesDto>> GetGroupesWithEmployeesByDateAndDeviceAsync(
        DateTime date,
        DeviceId deviceId,
        CancellationToken cancellationToken
    );
}
