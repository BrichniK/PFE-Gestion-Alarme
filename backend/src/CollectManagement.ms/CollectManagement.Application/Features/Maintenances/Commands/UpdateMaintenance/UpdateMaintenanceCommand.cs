namespace CollectManagement.Application.Features.Maintenances.Commands.UpdateMaintenance;

public record UpdateMaintenanceCommand(
    Ulid MaintenanceId,
    Ulid DeviceId,
    Ulid EmployeeId,
    DateTime? T1Alerte,
    DateTime? T2Assignment,
    DateTime? T3Arrival,
    DateTime? T4Completion,
    DateTime? T5Confirmation,
    DateTime? T6NextAlert,
    string Description
) : IRequest;
