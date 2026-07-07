namespace CollectManagement.Application.Features.Alertes.Queries.GetEmployeesByPlanning;

public record GetEmployeesByPlanningQuery(
    DateTime Date,
    Ulid DeviceId
) : IRequest<List<GroupeWithEmployeesDto>>;
