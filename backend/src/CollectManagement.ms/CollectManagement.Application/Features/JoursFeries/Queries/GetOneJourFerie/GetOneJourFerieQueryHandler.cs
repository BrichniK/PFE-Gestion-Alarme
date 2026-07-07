using CollectManagement.Application.Exceptions;
using CollectManagement.Application.Interfaces.Repositories.JoursFeries;
using CollectManagement.Domain.JoursFeries.ValueObjects;

namespace CollectManagement.Application.Features.JoursFeries.Queries.GetOneJourFerie;

public class GetOneJourFerieQueryHandler
    : IRequestHandler<GetOneJourFerieQuery, GetOneJourFerieResponse>
{
    private readonly IJourFerieRepository _jourFerieRepository;
    private readonly IMapper _mapper;

    public GetOneJourFerieQueryHandler(IJourFerieRepository jourFerieRepository, IMapper mapper)
    {
        _jourFerieRepository = jourFerieRepository;
        _mapper = mapper;
    }

    public async Task<GetOneJourFerieResponse> Handle(GetOneJourFerieQuery request, CancellationToken cancellationToken)
    {
        var jourFerieId = new JourFerieId(request.JourFerieId);

        var jourFerie = await _jourFerieRepository
            .GetOneAsync(jourFerieId, cancellationToken)
            .ConfigureAwait(false) ?? throw new NotFoundException("JourFerie NotFound.");

        return _mapper.Map<GetOneJourFerieResponse>(jourFerie);
    }
}
