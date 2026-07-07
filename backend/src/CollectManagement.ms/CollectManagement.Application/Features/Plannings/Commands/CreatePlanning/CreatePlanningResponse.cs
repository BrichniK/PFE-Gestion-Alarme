namespace CollectManagement.Application.Features.Plannings.Commands.CreatePlanning;

public record CreatePlanningResponse
{
    public Ulid PlanningId { get; init; }
    public IReadOnlyList<Ulid> PlanningIds { get; init; } = Array.Empty<Ulid>();
}
