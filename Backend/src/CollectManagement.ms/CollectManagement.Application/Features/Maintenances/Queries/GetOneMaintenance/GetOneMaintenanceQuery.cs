namespace CollectManagement.Application.Features.Maintenances.Queries.GetOneMaintenance;

public record GetOneMaintenanceQuery(Ulid MaintenanceId) : IRequest<GetOneMaintenanceResponse>;
