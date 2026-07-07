using CollectManagement.Application.Interfaces.Repositories.Types;

namespace CollectManagement.Application.Features.Types.Queries.GetPagedListType;

public class GetPagedListTypeQueryHandler
    : IRequestHandler<GetPagedListTypeQuery, GetPagedListTypeResponse>
{
    private readonly ITypeRepository _typeRepository;
    private readonly IMapper _mapper;

    public GetPagedListTypeQueryHandler(ITypeRepository typeRepository, IMapper mapper)
    {
        _typeRepository = typeRepository;
        _mapper = mapper;
    }

    public async Task<GetPagedListTypeResponse> Handle(GetPagedListTypeQuery request, CancellationToken cancellationToken)
    {
        var (listType, length) = await _typeRepository
            .GetPagedListAsync(
                request.Search,
                request.Sort,
                request.Order,
                request.Page,
                request.Size,
                cancellationToken
            )
            .ConfigureAwait(false);

        return new GetPagedListTypeResponse(
            _mapper.Map<List<GetPagedListTypeDto>>(listType),
            length
        );
    }
}
