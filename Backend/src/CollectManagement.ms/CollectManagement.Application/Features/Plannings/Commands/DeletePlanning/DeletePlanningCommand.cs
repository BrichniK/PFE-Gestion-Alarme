namespace CollectManagement.Application.Features.Plannings.Commands.DeletePlanning;

public record DeletePlanningCommand(Ulid PlanningId) : IRequest;
