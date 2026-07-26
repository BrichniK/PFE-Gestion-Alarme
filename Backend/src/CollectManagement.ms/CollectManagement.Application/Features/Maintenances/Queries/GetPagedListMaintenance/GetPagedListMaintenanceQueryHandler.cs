using CollectManagement.Application.Interfaces.Repositories.Maintenances;

namespace CollectManagement.Application.Features.Maintenances.Queries.GetPagedListMaintenance;

public class GetPagedListMaintenanceQueryHandler
    : IRequestHandler<GetPagedListMaintenanceQuery, GetPagedListMaintenanceResponse>
{
    private readonly IMaintenanceRepository _maintenanceRepository;
    private readonly IMapper _mapper;

    public GetPagedListMaintenanceQueryHandler(IMaintenanceRepository maintenanceRepository, IMapper mapper)
    {
        _maintenanceRepository = maintenanceRepository;
        _mapper = mapper;
    }

    public async Task<GetPagedListMaintenanceResponse> Handle(GetPagedListMaintenanceQuery request, CancellationToken cancellationToken)
    {
        var (listMaintenance, length) = await _maintenanceRepository
            .GetPagedListAsync(
                request.Search,
                request.Sort,
                request.Order,
                request.Page,
                request.Size,
                request.Filter,
                request.FromDate,
                request.ToDateExclusive,
                cancellationToken
            )
            .ConfigureAwait(false);

        return new GetPagedListMaintenanceResponse(
            _mapper.Map<List<GetPagedListMaintenanceDto>>(listMaintenance),
            length
        );
    }
}
