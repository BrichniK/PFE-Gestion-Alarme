namespace CollectManagement.Application.Features.Maintenances.Queries.GetMonitoringStats;

public record GetMonitoringStatsQuery(
    DateTime? StartDate,
    DateTime? EndDate,
    Ulid? DeviceId
) : IRequest<GetMonitoringStatsResponse>;
