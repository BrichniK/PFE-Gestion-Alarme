namespace CollectManagement.Application.Features.Maintenances.Commands.CreateMaintenance;

public record CreateMaintenanceCommand(
    Ulid DeviceId,
    Ulid EmployeeId,
    DateTime? T1Alerte,
    DateTime? T2Assignment,
    DateTime? T3Arrival,
    DateTime? T4Completion,
    DateTime? T5Confirmation,
    DateTime? T6NextAlert,
    string Description
) : IRequest<CreateMaintenanceResponse>;
