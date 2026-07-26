using CollectManagement.Application.Exceptions;
using CollectManagement.Application.Interfaces.Societes;
using CollectManagementDomain.Societes.ValueObjects;

namespace CollectManagement.Application.Features.Societes.Queries.GetOneSociete;

public class GetOneSocieteQueryHandler
    : IRequestHandler<GetOneSocieteQuery, GetOneSocieteResponse>
{
    private readonly ISocieteRepository _societeRepository;
    private readonly IMapper _mapper;

    public GetOneSocieteQueryHandler(ISocieteRepository societeRepository, IMapper mapper)
    {
        _societeRepository = societeRepository;
        _mapper = mapper;
    }

    public async Task<GetOneSocieteResponse> Handle(GetOneSocieteQuery request, CancellationToken cancellationToken)
    {
        var societeId = new SocieteId(request.SocieteId);

        var societe = await _societeRepository
            .GetOneAsync(societeId, cancellationToken)
            .ConfigureAwait(false)??throw new NotFoundException("Societe NotFound.");

        return _mapper.Map<GetOneSocieteResponse>(societe);
    }
}