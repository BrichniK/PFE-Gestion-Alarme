using CollectManagement.Application.Interfaces.Repositories.Alertes;
using CollectManagement.Domain.Alertes.ValueObjects;

namespace CollectManagement.Application.Features.Alertes.Commands.DeleteAlerte;

public class DeleteAlerteCommandHandler
    : IRequestHandler<DeleteAlerteCommand>
{
    private readonly IAlerteRepository _alerteRepository;

    public DeleteAlerteCommandHandler(IAlerteRepository alerteRepository)
    {
        _alerteRepository = alerteRepository;
    }

    public async Task Handle(DeleteAlerteCommand request, CancellationToken cancellationToken)
    {
        var alerteId = new AlerteId(request.AlerteId);

        await _alerteRepository
            .DeleteAsync(
                w => w.AlerteId.Equals(alerteId),
                cancellationToken
            )
            .ConfigureAwait(false);
    }
}
