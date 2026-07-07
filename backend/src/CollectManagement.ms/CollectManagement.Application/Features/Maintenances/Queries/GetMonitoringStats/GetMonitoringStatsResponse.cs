namespace CollectManagement.Application.Features.Maintenances.Queries.GetMonitoringStats;

public record GetMonitoringStatsResponse(
    double AvgD1Minutes,
    double AvgD2Minutes,
    double AvgD3Minutes,
    double AvgD4Minutes,
    double SumD1Minutes,
    double SumD2Minutes,
    double SumD3Minutes,
    double SumD4Minutes,
    double SumTotalMinutes,
    double MaxGaugeMinutes,
    int NbAlert
);
