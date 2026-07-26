using CollectManagement.Application.Interfaces.Repositories.Plannings;
using CollectManagement.Domain.Plannings.ValueObjects;

namespace CollectManagement.Application.Features.Plannings.Commands.DeletePlanning;

public class DeletePlanningCommandHandler
    : IRequestHandler<DeletePlanningCommand>
{
    private readonly IPlanningRepository _planningRepository;

    public DeletePlanningCommandHandler(IPlanningRepository planningRepository)
    {
        _planningRepository = planningRepository;
    }

    public async Task Handle(DeletePlanningCommand request, CancellationToken cancellationToken)
    {
        var planningId = new PlanningId(request.PlanningId);

        await _planningRepository
            .DeleteAsync(
                w => w.PlanningId.Equals(planningId),
                cancellationToken
            )
            .ConfigureAwait(false);
    }
}
