namespace CollectManagement.Application.Features.Plannings.Commands.UpdatePlanning;

public record UpdatePlanningCommand : IRequest
{
    public Ulid PlanningId { get; init; }
    public DateTime Date { get; init; }
    public Ulid GroupeId { get; init; }
    public Ulid DeviceId { get; init; }
    public Ulid ShiftId { get; init; }
    public IReadOnlyList<Ulid>? GroupeIds { get; init; }
    public IReadOnlyList<Ulid>? DeviceIds { get; init; }
    public IReadOnlyList<Ulid>? ShiftIds { get; init; }
    public Ulid EmployeeId { get; init; }
    public IReadOnlyList<Ulid>? EmployeeIds { get; init; }
}
