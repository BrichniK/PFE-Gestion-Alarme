namespace CollectManagement.Application.Features.Maintenances.Queries.GetMaintenanceStat;

public record GetMaintenanceStatQuery(
    string? Search,
    string? Sort,
    string? Order,
    int Page,
    int Size,
    DateTime? FromDate,
    DateTime? ToDateExclusive
) : IRequest<GetMaintenanceStatResponse>;
