using CollectManagement.Application.Interfaces.Repositories.Maintenances;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Employess.ObjectValues;
using CollectManagement.Domain.Maintenances;
using CollectManagement.Domain.Maintenances.ObjectValues;

namespace CollectManagement.Application.Features.Maintenances.Commands.UpdateMaintenance;

public class UpdateMaintenanceCommandHandler
    : IRequestHandler<UpdateMaintenanceCommand>
{
    private readonly IMaintenanceRepository _maintenanceRepository;

    public UpdateMaintenanceCommandHandler(IMaintenanceRepository maintenanceRepository)
    {
        _maintenanceRepository = maintenanceRepository;
    }

    public async Task Handle(UpdateMaintenanceCommand request, CancellationToken cancellationToken)
    {
        var maintenanceId = new MaintenanceId(request.MaintenanceId);
        var deviceId = new DeviceId(request.DeviceId);
        var employeeId = new EmployeeId(request.EmployeeId);

        var maintenance = Maintenance.Create(
            maintenanceId,
            deviceId,
            employeeId,
            request.T1Alerte,
            request.T2Assignment,
            request.T3Arrival,
            request.T4Completion,
            request.T5Confirmation,
            request.T6NextAlert,
            request.Description
        );

        await _maintenanceRepository.UpdateBulkAsync(maintenance, cancellationToken)
            .ConfigureAwait(false);
    }
}
