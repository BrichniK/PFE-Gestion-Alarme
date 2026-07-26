using CollectManagement.Application.Interfaces.Societes;

namespace CollectManagement.Application.Features.Societes.Queries.GetPagedListSociete;

public class GetPagedListSocieteQueryHandler
    : IRequestHandler<GetPagedListSocieteQuery, GetPagedListSocieteResponse>
{
    private readonly ISocieteRepository _societeRepository;
    private readonly IMapper _mapper;

    public GetPagedListSocieteQueryHandler(ISocieteRepository societeRepository, IMapper mapper)
    {
        _societeRepository = societeRepository;
        _mapper = mapper;
    }

    public async Task<GetPagedListSocieteResponse> Handle(GetPagedListSocieteQuery request, CancellationToken cancellationToken)
    {
        var (listSociete, length) = await _societeRepository
            .GetPagedListAsync(
                request.Search,
                request.Sort,
                request.Order,
                request.Page,
                request.Size,
                cancellationToken
            )
            .ConfigureAwait(false);

        return new GetPagedListSocieteResponse(
            _mapper.Map<List<GetPagedListSocieteDto>>(listSociete),
            length
        );
    }
}