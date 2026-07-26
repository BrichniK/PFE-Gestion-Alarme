using CollectManagement.Application.Interfaces.Repositories.Shifts;

namespace CollectManagement.Application.Features.Shifts.Queries.GetPagedListShift;

public class GetPagedListShiftQueryHandler
    : IRequestHandler<GetPagedListShiftQuery, GetPagedListShiftResponse>
{
    private readonly IShiftRepository _shiftRepository;
    private readonly IMapper _mapper;

    public GetPagedListShiftQueryHandler(IShiftRepository shiftRepository, IMapper mapper)
    {
        _shiftRepository = shiftRepository;
        _mapper = mapper;
    }

    public async Task<GetPagedListShiftResponse> Handle(GetPagedListShiftQuery request, CancellationToken cancellationToken)
    {
        var (listShift, length) = await _shiftRepository
            .GetPagedListAsync(
                request.Search,
                request.Sort,
                request.Order,
                request.Page,
                request.Size,
                cancellationToken
            )
            .ConfigureAwait(false);

        return new GetPagedListShiftResponse(
            _mapper.Map<List<GetPagedListShiftDto>>(listShift),
            length
        );
    }
}
