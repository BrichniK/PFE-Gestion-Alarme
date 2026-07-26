namespace CollectManagement.Application.Features.Maintenances.Queries.GetMaintenanceStat;

public record GetMaintenanceStatResponse(
    List<GetMaintenanceStatDto> Stats,
    int Length
);
