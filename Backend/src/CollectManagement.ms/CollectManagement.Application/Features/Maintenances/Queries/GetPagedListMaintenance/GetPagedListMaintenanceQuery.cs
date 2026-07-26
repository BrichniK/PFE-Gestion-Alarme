namespace CollectManagement.Application.Features.Maintenances.Queries.GetPagedListMaintenance;

public record GetPagedListMaintenanceQuery(
    string? Search,
    string? Sort,
    string? Order,
    int Page,
    int Size,
    string? Filter,
    DateTime? FromDate,
    DateTime? ToDateExclusive
) : IRequest<GetPagedListMaintenanceResponse>;
