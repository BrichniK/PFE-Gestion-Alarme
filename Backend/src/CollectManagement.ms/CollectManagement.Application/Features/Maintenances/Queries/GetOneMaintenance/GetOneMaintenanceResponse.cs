namespace CollectManagement.Application.Features.Maintenances.Queries.GetOneMaintenance;

public record GetOneMaintenanceResponse(
    Ulid MaintenanceId,
    Ulid DeviceId,
    string? DeviceName,
    Ulid EmployeeId,
    string? EmployeeNom,
    string? EmployeePrenom,
    DateTime? T1Alerte,
    DateTime? T2Assignment,
    DateTime? T3Arrival,
    DateTime? T4Completion,
    DateTime? T5Confirmation,
    DateTime? T6NextAlert,
    string Description
);
