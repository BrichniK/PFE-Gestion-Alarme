using CollectManagement.Application.Exceptions;
using CollectManagement.Application.Interfaces.Repositories.Plannings;
using CollectManagement.Domain.Plannings.ValueObjects;

namespace CollectManagement.Application.Features.Plannings.Queries.GetOnePlanning;

public class GetOnePlanningQueryHandler
    : IRequestHandler<GetOnePlanningQuery, GetOnePlanningResponse>
{
    private readonly IPlanningRepository _planningRepository;
    private readonly IMapper _mapper;

    public GetOnePlanningQueryHandler(IPlanningRepository planningRepository, IMapper mapper)
    {
        _planningRepository = planningRepository;
        _mapper = mapper;
    }

    public async Task<GetOnePlanningResponse> Handle(GetOnePlanningQuery request, CancellationToken cancellationToken)
    {
        var planningId = new PlanningId(request.PlanningId);

        var planning = await _planningRepository
            .GetOneAsync(planningId, cancellationToken)
            .ConfigureAwait(false) ?? throw new NotFoundException("Planning NotFound.");

        return _mapper.Map<GetOnePlanningResponse>(planning);
    }
}
