using CollectManagement.Application.Interfaces.Repositories.Plannings;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Employess.ObjectValues;
using CollectManagement.Domain.Groupes.ValueObjects;
using CollectManagement.Domain.Plannings;
using CollectManagement.Domain.Plannings.ValueObjects;
using CollectManagement.Domain.Shifts.ValueObjects;


namespace CollectManagement.Application.Features.Plannings.Commands.CreatePlanning;

public class CreatePlanningCommandHandler
    : IRequestHandler<CreatePlanningCommand, CreatePlanningResponse>
{
    private readonly IPlanningRepository _planningRepository;

    public CreatePlanningCommandHandler(
        IPlanningRepository planningRepository)
    {
        _planningRepository = planningRepository;
    }

    public async Task<CreatePlanningResponse> Handle(CreatePlanningCommand request, CancellationToken cancellationToken)
    {
        var groupeIds = NormalizeIds(request.GroupeIds, request.GroupeId)
            .Select(x => new GroupeId(x));
        var deviceIds = NormalizeIds(request.DeviceIds, request.DeviceId)
            .Select(x => new DeviceId(x));
        var shiftIds = NormalizeIds(request.ShiftIds, request.ShiftId)
            .Select(x => new ShiftId(x));
        var employeeIds = NormalizeIds(request.EmployeeIds, request.EmployeeId)
            .Select(x => new EmployeeId(x));

        var dates = NormalizeDates(request.Dates, request.Date);
        var plannings = dates
            .Select(date => Planning.Create(
                new PlanningId(Ulid.NewUlid()),
                date,
                groupeIds,
                deviceIds,
                shiftIds,
                employeeIds
            ))
            .ToList();

        if (plannings.Count == 1)
        {
            await _planningRepository
                .AddAsync(plannings[0], cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            await _planningRepository
                .AddRangeAsync(plannings, cancellationToken)
                .ConfigureAwait(false);
        }

        var planningIds = plannings.Select(p => p.PlanningId.Value).ToList();
        return new CreatePlanningResponse
        {
            PlanningId = planningIds.Count > 0 ? planningIds[0] : Ulid.Empty,
            PlanningIds = planningIds,
        };
    }

    private static IReadOnlyList<Ulid> NormalizeIds(IReadOnlyList<Ulid>? ids, Ulid singleId)
    {
        if (ids is { Count: > 0 })
            return ids;

        return singleId == Ulid.Empty
            ? Array.Empty<Ulid>()
            : new[] { singleId };
    }

    private static IReadOnlyList<DateTime> NormalizeDates(IReadOnlyList<DateTime>? dates, DateTime singleDate)
    {
        if (dates is { Count: > 0 })
        {
            return dates
                .Select(date => date.Date)
                .Distinct()
                .ToList();
        }

        return singleDate == default
            ? Array.Empty<DateTime>()
            : new[] { singleDate.Date };
    }
}
