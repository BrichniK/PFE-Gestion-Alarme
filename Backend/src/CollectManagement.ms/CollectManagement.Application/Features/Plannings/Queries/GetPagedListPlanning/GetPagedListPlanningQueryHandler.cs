using CollectManagement.Application.Interfaces.Repositories.Plannings;

namespace CollectManagement.Application.Features.Plannings.Queries.GetPagedListPlanning;

public class GetPagedListPlanningQueryHandler
    : IRequestHandler<GetPagedListPlanningQuery, GetPagedListPlanningResponse>
{
    private readonly IPlanningRepository _planningRepository;
    private readonly IMapper _mapper;

    public GetPagedListPlanningQueryHandler(IPlanningRepository planningRepository, IMapper mapper)
    {
        _planningRepository = planningRepository;
        _mapper = mapper;
    }

    public async Task<GetPagedListPlanningResponse> Handle(GetPagedListPlanningQuery request, CancellationToken cancellationToken)
    {
        var (listPlanning, length) = await _planningRepository
            .GetPagedListAsync(
                request.Search,
                request.Sort,
                request.Order,
                request.Page,
                request.Size,
                cancellationToken
            )
            .ConfigureAwait(false);

        return new GetPagedListPlanningResponse(
            _mapper.Map<List<GetPagedListPlanningDto>>(listPlanning),
            length
        );
    }
}
