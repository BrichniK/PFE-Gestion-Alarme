namespace CollectManagement.Application.Features.Plannings.Queries.GetPagedListPlanning;

public record GetPagedListPlanningResponse(
    List<GetPagedListPlanningDto> Plannings,
    int Length
);
