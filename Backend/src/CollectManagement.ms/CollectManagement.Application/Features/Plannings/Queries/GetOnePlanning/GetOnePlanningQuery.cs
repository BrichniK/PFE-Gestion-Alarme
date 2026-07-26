namespace CollectManagement.Application.Features.Plannings.Queries.GetOnePlanning;

public record GetOnePlanningQuery(Ulid PlanningId) : IRequest<GetOnePlanningResponse>;
