using CollectManagement.Application.Exceptions;
using CollectManagement.Application.Interfaces.Repositories.Alertes;
using CollectManagement.Domain.Alertes.ValueObjects;

namespace CollectManagement.Application.Features.Alertes.Queries.GetOneAlerte;

public class GetOneAlerteQueryHandler
    : IRequestHandler<GetOneAlerteQuery, GetOneAlerteResponse>
{
    private readonly IAlerteRepository _alerteRepository;
    private readonly IMapper _mapper;

    public GetOneAlerteQueryHandler(IAlerteRepository alerteRepository, IMapper mapper)
    {
        _alerteRepository = alerteRepository;
        _mapper = mapper;
    }

    public async Task<GetOneAlerteResponse> Handle(GetOneAlerteQuery request, CancellationToken cancellationToken)
    {
        var alerteId = new AlerteId(request.AlerteId);

        var alerte = await _alerteRepository
            .GetOneAsync(alerteId, cancellationToken)
            .ConfigureAwait(false) ?? throw new NotFoundException("Alerte NotFound.");

        return _mapper.Map<GetOneAlerteResponse>(alerte);
    }
}
