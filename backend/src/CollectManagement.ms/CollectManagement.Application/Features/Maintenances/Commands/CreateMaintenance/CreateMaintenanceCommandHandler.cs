using CollectManagement.Application.Interfaces.Repositories.Maintenances;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Employess.ObjectValues;
using CollectManagement.Domain.Maintenances;
using CollectManagement.Domain.Maintenances.ObjectValues;

namespace CollectManagement.Application.Features.Maintenances.Commands.CreateMaintenance;

public class CreateMaintenanceCommandHandler
    : IRequestHandler<CreateMaintenanceCommand, CreateMaintenanceResponse>
{
    private readonly IMaintenanceRepository _maintenanceRepository;
    private readonly IMapper _mapper;

    public CreateMaintenanceCommandHandler(
        IMaintenanceRepository maintenanceRepository,
        IMapper mapper)
    {
        _maintenanceRepository = maintenanceRepository;
        _mapper = mapper;
    }

    public async Task<CreateMaintenanceResponse> Handle(CreateMaintenanceCommand request, CancellationToken cancellationToken)
    {
        var maintenanceId = new MaintenanceId(Ulid.NewUlid());
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

        await _maintenanceRepository
            .AddAsync(maintenance, cancellationToken)
            .ConfigureAwait(false);

        // Set T6NextAlert on the previous maintenance for the same device
        if (request.T1Alerte.HasValue)
        {
            var previousMaintenance = await _maintenanceRepository.GetLatestByDeviceIdAsync(deviceId, cancellationToken);
            if (previousMaintenance != null && previousMaintenance.MaintenanceId != maintenanceId)
            {
                previousMaintenance.SetT6NextAlert(request.T1Alerte.Value);
                await _maintenanceRepository.UpdateBulkAsync(previousMaintenance, cancellationToken);
            }
        }

        return _mapper.Map<CreateMaintenanceResponse>(maintenance);
    }
}
