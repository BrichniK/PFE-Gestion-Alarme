using CollectManagement.Application.Exceptions;
using CollectManagement.Application.Interfaces.Repositories.Maintenances;
using CollectManagement.Domain.Maintenances.ObjectValues;

namespace CollectManagement.Application.Features.Maintenances.Queries.GetOneMaintenance;

public class GetOneMaintenanceQueryHandler
    : IRequestHandler<GetOneMaintenanceQuery, GetOneMaintenanceResponse>
{
    private readonly IMaintenanceRepository _maintenanceRepository;
    private readonly IMapper _mapper;

    public GetOneMaintenanceQueryHandler(IMaintenanceRepository maintenanceRepository, IMapper mapper)
    {
        _maintenanceRepository = maintenanceRepository;
        _mapper = mapper;
    }

    public async Task<GetOneMaintenanceResponse> Handle(GetOneMaintenanceQuery request, CancellationToken cancellationToken)
    {
        var maintenanceId = new MaintenanceId(request.MaintenanceId);

        var maintenance = await _maintenanceRepository
            .GetOneAsync(maintenanceId, cancellationToken)
            .ConfigureAwait(false) ?? throw new NotFoundException("Maintenance NotFound.");

        return _mapper.Map<GetOneMaintenanceResponse>(maintenance);
    }
}
