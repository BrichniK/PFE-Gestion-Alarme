namespace CollectManagement.Application.Features.Maintenances.Commands.DeleteMaintenance;

public record DeleteMaintenanceCommand(Ulid MaintenanceId) : IRequest;
