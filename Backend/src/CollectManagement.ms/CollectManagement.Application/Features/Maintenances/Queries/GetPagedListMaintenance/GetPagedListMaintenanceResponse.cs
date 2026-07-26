namespace CollectManagement.Application.Features.Maintenances.Queries.GetPagedListMaintenance;

public record GetPagedListMaintenanceResponse(
    List<GetPagedListMaintenanceDto> Maintenances,
    int Length
);
