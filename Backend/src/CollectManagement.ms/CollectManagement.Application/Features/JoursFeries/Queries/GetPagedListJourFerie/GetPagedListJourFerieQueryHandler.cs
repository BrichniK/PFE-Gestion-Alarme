using CollectManagement.Application.Interfaces.Repositories.JoursFeries;

namespace CollectManagement.Application.Features.JoursFeries.Queries.GetPagedListJourFerie;

public class GetPagedListJourFerieQueryHandler
    : IRequestHandler<GetPagedListJourFerieQuery, GetPagedListJourFerieResponse>
{
    private readonly IJourFerieRepository _jourFerieRepository;
    private readonly IMapper _mapper;

    public GetPagedListJourFerieQueryHandler(IJourFerieRepository jourFerieRepository, IMapper mapper)
    {
        _jourFerieRepository = jourFerieRepository;
        _mapper = mapper;
    }

    public async Task<GetPagedListJourFerieResponse> Handle(GetPagedListJourFerieQuery request, CancellationToken cancellationToken)
    {
        var (listJourFerie, length) = await _jourFerieRepository
            .GetPagedListAsync(
                request.Search,
                request.Sort,
                request.Order,
                request.Page,
                request.Size,
                cancellationToken
            )
            .ConfigureAwait(false);

        return new GetPagedListJourFerieResponse(
            _mapper.Map<List<GetPagedListJourFerieDto>>(listJourFerie),
            length
        );
    }
}
