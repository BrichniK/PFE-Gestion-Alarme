using CollectManagement.Application.Exceptions;
using CollectManagement.Application.Interfaces.Repositories.Plannings;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Employess.ObjectValues;
using CollectManagement.Domain.Groupes.ValueObjects;
using CollectManagement.Domain.Plannings.ValueObjects;
using CollectManagement.Domain.Shifts.ValueObjects;


namespace CollectManagement.Application.Features.Plannings.Commands.UpdatePlanning;

public class UpdatePlanningCommandHandler
    : IRequestHandler<UpdatePlanningCommand>
{
    private readonly IPlanningRepository _planningRepository;

    public UpdatePlanningCommandHandler(IPlanningRepository planningRepository)
    {
        _planningRepository = planningRepository;
    }

    public async Task Handle(UpdatePlanningCommand request, CancellationToken cancellationToken)
    {
        var planningId = new PlanningId(request.PlanningId);
        var planning = await _planningRepository
            .GetOneAsync(planningId, cancellationToken)
            .ConfigureAwait(false) ?? throw new NotFoundException("Planning NotFound.");

        var groupeIds = NormalizeIds(request.GroupeIds, request.GroupeId)
            .Select(x => new GroupeId(x));
        var deviceIds = NormalizeIds(request.DeviceIds, request.DeviceId)
            .Select(x => new DeviceId(x));
        var shiftIds = NormalizeIds(request.ShiftIds, request.ShiftId)
            .Select(x => new ShiftId(x));
        var employeeIds = NormalizeIds(request.EmployeeIds, request.EmployeeId)
            .Select(x => new EmployeeId(x));

        planning.Update(
            request.Date,
            groupeIds,
            deviceIds,
            shiftIds,
            employeeIds
        );
    }

    private static IReadOnlyList<Ulid> NormalizeIds(IReadOnlyList<Ulid>? ids, Ulid singleId)
    {
        if (ids is { Count: > 0 })
            return ids;

        return singleId == Ulid.Empty
            ? Array.Empty<Ulid>()
            : new[] { singleId };
    }
}
