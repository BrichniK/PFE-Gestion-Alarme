namespace CollectManagement.Application.Features.Plannings.Queries.GetPagedListPlanning;

public record GetPagedListPlanningQuery(
    string? Search,
    string? Sort,
    string? Order,
    int Page,
    int Size
) : IRequest<GetPagedListPlanningResponse>;
