using CollectManagement.Application.Interfaces.Repositories.Alertes;

namespace CollectManagement.Application.Features.Alertes.Queries.GetPagedListAlerte;

public class GetPagedListAlerteQueryHandler
    : IRequestHandler<GetPagedListAlerteQuery, GetPagedListAlerteResponse>
{
    private readonly IAlerteRepository _alerteRepository;
    private readonly IMapper _mapper;

    public GetPagedListAlerteQueryHandler(IAlerteRepository alerteRepository, IMapper mapper)
    {
        _alerteRepository = alerteRepository;
        _mapper = mapper;
    }

    public async Task<GetPagedListAlerteResponse> Handle(GetPagedListAlerteQuery request, CancellationToken cancellationToken)
    {
        var (listAlerte, length) = await _alerteRepository
            .GetPagedListAsync(
                request.Search,
                request.Sort,
                request.Order,
                request.Page,
                request.Size,
                cancellationToken
            )
            .ConfigureAwait(false);

        return new GetPagedListAlerteResponse(
            _mapper.Map<List<GetPagedListAlerteDto>>(listAlerte),
            length
        );
    }
}
