using CollectManagement.Application.Interfaces.Repositories.Plannings;
using CollectManagement.Domain.Devices.ValueObjects;

namespace CollectManagement.Application.Features.Alertes.Queries.GetEmployeesByPlanning;

public class GetEmployeesByPlanningQueryHandler
    : IRequestHandler<GetEmployeesByPlanningQuery, List<GroupeWithEmployeesDto>>
{
    private readonly IPlanningRepository _planningRepository;

    public GetEmployeesByPlanningQueryHandler(IPlanningRepository planningRepository)
    {
        _planningRepository = planningRepository;
    }

    public async Task<List<GroupeWithEmployeesDto>> Handle(GetEmployeesByPlanningQuery request, CancellationToken cancellationToken)
    {
        var deviceId = new DeviceId(request.DeviceId);
        var groupes = await _planningRepository.GetGroupesWithEmployeesByDateAndDeviceAsync(
            request.Date.Date,
            deviceId,
            cancellationToken);

        return groupes;
    }
}
