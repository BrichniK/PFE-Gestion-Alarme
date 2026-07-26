namespace CollectManagement.Application.Features.Maintenances.Queries.GetMaintenanceStat;

public record GetMaintenanceStatDto(
    Ulid MaintenanceId,
    Ulid DeviceId,
    Ulid EmployeeId,
    string? DeviceName,
    string? EmployeeName,
    DateTime? T3Arrival,
    DateTime? T4Completion,
    double DureeReel,
    double? DureeTotalAlerte,
    string? TypeLabel,
    int? DureeNominal,
    double? Ecart,
    bool IsDepassement
);
