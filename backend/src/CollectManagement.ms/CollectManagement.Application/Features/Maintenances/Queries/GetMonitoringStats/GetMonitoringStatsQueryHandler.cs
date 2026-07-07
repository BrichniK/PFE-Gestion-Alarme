using CollectManagement.Application.Interfaces.Repositories.Maintenances;

namespace CollectManagement.Application.Features.Maintenances.Queries.GetMonitoringStats;

public class GetMonitoringStatsQueryHandler
    : IRequestHandler<GetMonitoringStatsQuery, GetMonitoringStatsResponse>
{
    private readonly IMaintenanceRepository _maintenanceRepository;

    public GetMonitoringStatsQueryHandler(IMaintenanceRepository maintenanceRepository)
    {
        _maintenanceRepository = maintenanceRepository;
    }

    public async Task<GetMonitoringStatsResponse> Handle(GetMonitoringStatsQuery request, CancellationToken cancellationToken)
    {
        var startDate = request.StartDate ?? DateTime.MinValue;
        var endDate = request.EndDate?.AddDays(1) ?? DateTime.MaxValue;

        var all = await _maintenanceRepository
            .GetByDateRangeAsync(startDate, endDate, request.DeviceId, cancellationToken)
            .ConfigureAwait(false);

        // Gauge formulas are based on completed maintenances where total duration exists (T5 - T1).
        var maintenances = all
            .Where(m => m.T1Alerte.HasValue && m.T5Confirmation.HasValue)
            .ToList();

        var nbAlert = maintenances.Count;

        if (nbAlert == 0)
        {
            return new GetMonitoringStatsResponse(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }

        // D1 = T2 - T1 (assignment duration)
        // D2 = T3 - T2 (waiting duration)
        // D3 = T4 - T3 (diagnostic duration)
        // D4 = T5 - T4 (repair duration)
        double sumD1 = 0, sumD2 = 0, sumD3 = 0, sumD4 = 0, sumTotal = 0;

        foreach (var m in maintenances)
        {
            var total = (m.T5Confirmation!.Value - m.T1Alerte!.Value).TotalMinutes;
            if (total > 0)
            {
                sumTotal += total;
            }

            if (m.T1Alerte.HasValue && m.T2Assignment.HasValue)
            {
                var d1 = (m.T2Assignment.Value - m.T1Alerte.Value).TotalMinutes;
                if (d1 > 0)
                {
                    sumD1 += d1;
                }
            }

            if (m.T2Assignment.HasValue && m.T3Arrival.HasValue)
            {
                var d2 = (m.T3Arrival.Value - m.T2Assignment.Value).TotalMinutes;
                if (d2 > 0)
                {
                    sumD2 += d2;
                }
            }

            if (m.T3Arrival.HasValue && m.T4Completion.HasValue)
            {
                var d3 = (m.T4Completion.Value - m.T3Arrival.Value).TotalMinutes;
                if (d3 > 0)
                {
                    sumD3 += d3;
                }
            }

            if (m.T4Completion.HasValue && m.T5Confirmation.HasValue)
            {
                var d4 = (m.T5Confirmation.Value - m.T4Completion.Value).TotalMinutes;
                if (d4 > 0)
                {
                    sumD4 += d4;
                }
            }
        }

        var avgD1 = Math.Round(sumD1 / nbAlert, 2);
        var avgD2 = Math.Round(sumD2 / nbAlert, 2);
        var avgD3 = Math.Round(sumD3 / nbAlert, 2);
        var avgD4 = Math.Round(sumD4 / nbAlert, 2);
        var maxGauge = Math.Round(sumTotal / nbAlert, 2);

        return new GetMonitoringStatsResponse(
            avgD1,
            avgD2,
            avgD3,
            avgD4,
            Math.Round(sumD1, 2),
            Math.Round(sumD2, 2),
            Math.Round(sumD3, 2),
            Math.Round(sumD4, 2),
            Math.Round(sumTotal, 2),
            maxGauge,
            nbAlert);
    }
}
