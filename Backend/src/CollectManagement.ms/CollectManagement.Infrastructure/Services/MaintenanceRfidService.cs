using CollectManagement.Application.Interfaces.Repositories.Maintenances;
using CollectManagement.Application.Interfaces.Services;

namespace CollectManagement.Infrastructure.Services;

public class MaintenanceRfidService : IMaintenanceRfidService
{
    private readonly IMaintenanceRepository _maintenanceRepository;

    public MaintenanceRfidService(IMaintenanceRepository maintenanceRepository)
    {
        _maintenanceRepository = maintenanceRepository ?? throw new ArgumentNullException(nameof(maintenanceRepository));
    }

    public async Task<MaintenanceRfidResponse> HandleRfidScanAsync(string rfid, CancellationToken cancellationToken)
    {
        // Validate RFID input
        if (string.IsNullOrWhiteSpace(rfid))
        {
            return new MaintenanceRfidResponse
            {
                Success = false,
                Message = "Le tag RFID est requis."
            };
        }

        // Find active maintenance for this RFID
        var maintenance = await _maintenanceRepository.GetActiveByEmployeeRfidAsync(rfid, cancellationToken);

        if (maintenance == null)
        {
            return new MaintenanceRfidResponse
            {
                Success = false,
                Message = $"Aucune maintenance active trouvée pour le RFID '{rfid}'. " +
                          "Vérifiez que l'employé existe et qu'une maintenance lui est assignée.",
                EmployeeRfid = rfid
            };
        }

        // Check if already fully completed
        if (maintenance.IsCompleted)
        {
            return new MaintenanceRfidResponse
            {
                Success = false,
                Message = "Cette maintenance est déjà complètement terminée (T1-T4 tous remplis).",
                EmployeeNom = maintenance.Employee?.Nom,
                EmployeePrenom = maintenance.Employee?.Prenom,
                EmployeeRfid = rfid,
                MaintenanceId = maintenance.MaintenanceId.Value,
                T1Alerte = maintenance.T1Alerte,
                T2Assignment = maintenance.T2Assignment,
                T3Arrival = maintenance.T3Arrival,
                T4Completion = maintenance.T4Completion
            };
        }

        // Process the RFID scan - advance to next T step
        var stepCompleted = maintenance.ProcessRfidScan();

        // Persist the update
        await _maintenanceRepository.UpdateBulkAsync(maintenance, cancellationToken);

        // Determine next step
        var nextStep = maintenance.CurrentStep;

        return new MaintenanceRfidResponse
        {
            Success = true,
            Message = $"Étape {stepCompleted} validée avec succès pour {maintenance.Employee?.Nom} {maintenance.Employee?.Prenom}.",
            StepCompleted = stepCompleted,
            NextStep = nextStep,
            EmployeeNom = maintenance.Employee?.Nom,
            EmployeePrenom = maintenance.Employee?.Prenom,
            EmployeeRfid = rfid,
            MaintenanceId = maintenance.MaintenanceId.Value,
            T1Alerte = maintenance.T1Alerte,
            T2Assignment = maintenance.T2Assignment,
            T3Arrival = maintenance.T3Arrival,
            T4Completion = maintenance.T4Completion
        };
    }
}
