using CollectManagement.Application.Interfaces.Repositories.Maintenances;
using CollectManagement.Domain.Maintenances.ObjectValues;

namespace CollectManagement.Application.Features.Maintenances.Commands.DeleteMaintenance;

public class DeleteMaintenanceCommandHandler
    : IRequestHandler<DeleteMaintenanceCommand>
{
    private readonly IMaintenanceRepository _maintenanceRepository;

    public DeleteMaintenanceCommandHandler(IMaintenanceRepository maintenanceRepository)
    {
        _maintenanceRepository = maintenanceRepository;
    }

    public async Task Handle(DeleteMaintenanceCommand request, CancellationToken cancellationToken)
    {
        var maintenanceId = new MaintenanceId(request.MaintenanceId);

        await _maintenanceRepository
            .DeleteAsync(
                w => w.MaintenanceId.Equals(maintenanceId),
                cancellationToken
            )
            .ConfigureAwait(false);
    }
}
