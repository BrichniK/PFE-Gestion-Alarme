using CollectManagement.Application.Interfaces.Repositories.Maintenances;

namespace CollectManagement.Application.Features.Maintenances.Queries.GetKpiIndicators;

public class GetKpiIndicatorsQueryHandler
    : IRequestHandler<GetKpiIndicatorsQuery, GetKpiIndicatorsResponse>
{
    private const string ZeroDuration = "00:00:00:00";
    private readonly IMaintenanceRepository _maintenanceRepository;

    public GetKpiIndicatorsQueryHandler(IMaintenanceRepository maintenanceRepository)
    {
        _maintenanceRepository = maintenanceRepository;
    }

    public async Task<GetKpiIndicatorsResponse> Handle(
        GetKpiIndicatorsQuery request,
        CancellationToken cancellationToken)
    {
        var startDate = request.StartDate ?? DateTime.MinValue;
        var endDate = request.EndDate?.AddDays(1) ?? DateTime.MaxValue;

        var all = await _maintenanceRepository
            .GetByDateRangeAsync1(startDate, endDate, cancellationToken)
            .ConfigureAwait(false);

        // Only include maintenances where T6NextAlert is set
        var maintenances = all.Where(m => m.T6NextAlert.HasValue).ToList();

        if (request.DeviceId.HasValue)
        {
            maintenances = maintenances
                .Where(maintenance => maintenance.DeviceId.Value == request.DeviceId.Value)
                .ToList();
        }

        var nbAlert = maintenances.Count;
        if (nbAlert == 0)
        {
            return new GetKpiIndicatorsResponse(
                ZeroDuration,
                ZeroDuration,
                ZeroDuration,
                ZeroDuration,
                0,
                0);
        }

        long sumD1Ticks = 0;
        long sumD2Ticks = 0;
        long sumD3Ticks = 0;
        long sumD4Ticks = 0;
        long sumMttfTicks = 0;
        var nbPannes = 0;

        foreach (var maintenance in maintenances)
        {
            var d1 = DiffOrZero(maintenance.T1Alerte, maintenance.T2Assignment);
            var d2 = DiffOrZero(maintenance.T2Assignment, maintenance.T3Arrival);
            var d3 = DiffOrZero(maintenance.T3Arrival, maintenance.T4Completion);
            var d4 = DiffOrZero(maintenance.T4Completion, maintenance.T5Confirmation);

            sumD1Ticks += d1.Ticks;
            sumD2Ticks += d2.Ticks;
            sumD3Ticks += d3.Ticks;
            sumD4Ticks += d4.Ticks;

            var d5 = DiffOrZero(maintenance.T5Confirmation, maintenance.T6NextAlert);
            sumMttfTicks += d5.Ticks;
            if (d5 > TimeSpan.Zero) nbPannes++;
        }

        // MTTD = (D1 + D2 + D3) / nbAlert
        var mttdTicks = (sumD1Ticks + sumD2Ticks + sumD3Ticks) / nbAlert;
        // MTTR = D4 / nbAlert
        var mttrTicks = sumD4Ticks / nbAlert;
        // MTTF = sum(D5) / nbPannes (only records where T6 is not null)
        var mttfTicks = nbPannes > 0 ? sumMttfTicks / nbPannes : 0;
        // MTBF = MTTD + MTTR + MTTF
        var mtbfTicks = mttdTicks + mttrTicks + mttfTicks;

        return new GetKpiIndicatorsResponse(
            FormatDuration(TimeSpan.FromTicks(mttrTicks)),
            FormatDuration(TimeSpan.FromTicks(mttdTicks)),
            FormatDuration(TimeSpan.FromTicks(mttfTicks)),
            FormatDuration(TimeSpan.FromTicks(mtbfTicks)),
            nbAlert,
            nbPannes);
    }

    private static TimeSpan DiffOrZero(DateTime? from, DateTime? to)
    {
        if (!from.HasValue || !to.HasValue || to.Value < from.Value)
        {
            return TimeSpan.Zero;
        }

        return to.Value - from.Value;
    }

    private static string FormatDuration(TimeSpan value)
    {
        var days = Math.Max(0, value.Days);
        return $"{days:00}:{value.Hours:00}:{value.Minutes:00}:{value.Seconds:00}";
    }
}
