using CollectManagement.Application.Interfaces.Groupes;

namespace CollectManagement.Application.Features.Groupes.Queries.GetPagedListGroupe;

public class GetPagedListGroupeQueryHandler
    : IRequestHandler<GetPagedListGroupeQuery, GetPagedListGroupeResponse>
{
    private readonly IGroupeRepository _groupeRepository;
    private readonly IMapper _mapper;

    public GetPagedListGroupeQueryHandler(IGroupeRepository groupeRepository, IMapper mapper)
    {
        _groupeRepository = groupeRepository;
        _mapper = mapper;
    }

    public async Task<GetPagedListGroupeResponse> Handle(GetPagedListGroupeQuery request, CancellationToken cancellationToken)
    {
        var (listGroupe, length) = await _groupeRepository
            .GetPagedListAsync(
                request.Search,
                request.Sort,
                request.Order,
                request.Page,
                request.Size,
                cancellationToken
            )
            .ConfigureAwait(false);

        return new GetPagedListGroupeResponse(
            _mapper.Map<List<GetPagedListGroupeDto>>(listGroupe),
            length
        );
    }
}
