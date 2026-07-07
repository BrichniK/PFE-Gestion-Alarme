using CollectManagement.Application.Interfaces.Repositories.Alertes;
using CollectManagement.Application.Interfaces.Repositories.Maintenances;
using CollectManagement.Domain.Devices.ValueObjects;

namespace CollectManagement.Application.Features.Maintenances.Queries.GetMaintenanceStat;

public class GetMaintenanceStatQueryHandler
    : IRequestHandler<GetMaintenanceStatQuery, GetMaintenanceStatResponse>
{
    private readonly IMaintenanceRepository _maintenanceRepository;
    private readonly IAlerteRepository _alerteRepository;

    public GetMaintenanceStatQueryHandler(
        IMaintenanceRepository maintenanceRepository,
        IAlerteRepository alerteRepository)
    {
        _maintenanceRepository = maintenanceRepository;
        _alerteRepository = alerteRepository;
    }

    public async Task<GetMaintenanceStatResponse> Handle(
        GetMaintenanceStatQuery request,
        CancellationToken cancellationToken)
    {
        var (maintenances, total) = await _maintenanceRepository
            .GetCompletedPagedListAsync(request.Search, request.Page, request.Size, request.FromDate, request.ToDateExclusive, cancellationToken)
            .ConfigureAwait(false);

        var stats = new List<GetMaintenanceStatDto>();

        foreach (var m in maintenances)
        {
            var dureeReel = (m.T4Completion!.Value - m.T3Arrival!.Value).TotalSeconds;
            dureeReel = Math.Round(dureeReel, 2);

            double? dureeTotalAlerte = null;
            if (m.T1Alerte.HasValue)
            {
                dureeTotalAlerte = Math.Round((m.T4Completion!.Value - m.T1Alerte.Value).TotalSeconds, 2);
            }

            string? typeLabel = null;
            int? dureeNominal = null;
            double? ecart = null;
            bool isDepassement = false;

            // Try to find the corresponding alert using DeviceId + T1Alerte date
            if (m.T1Alerte.HasValue)
            {
                var alerte = await _alerteRepository
                    .GetByDeviceIdAndDateAsync(m.DeviceId, m.T1Alerte.Value, cancellationToken)
                    .ConfigureAwait(false);

                if (alerte?.Type != null)
                {
                    typeLabel = alerte.Type.Label;
                    dureeNominal = alerte.Type.DureeNominal;

                    if (dureeNominal.HasValue)
                    {
                        ecart = Math.Round(dureeReel - dureeNominal.Value, 2);
                        isDepassement = dureeReel > dureeNominal.Value;
                    }
                }
            }

            var employeeName = m.Employee != null
                ? $"{m.Employee.Nom} {m.Employee.Prenom}"
                : null;

            stats.Add(new GetMaintenanceStatDto(
                m.MaintenanceId.Value,
                m.DeviceId.Value,
                m.EmployeeId.Value,
                m.Device?.DeviceName,
                employeeName,
                m.T3Arrival,
                m.T4Completion,
                dureeReel,
                dureeTotalAlerte,
                typeLabel,
                dureeNominal,
                ecart,
                isDepassement
            ));
        }

        return new GetMaintenanceStatResponse(stats, total);
    }
}
