using CollectManagement.Application.Interfaces.Repositories.Alertes;
using CollectManagement.Domain.Alertes;
using CollectManagement.Domain.Alertes.ValueObjects;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Types.ValueObjects;

namespace CollectManagement.Application.Features.Alertes.Commands.UpdateAlerte;

public class UpdateAlerteCommandHandler
    : IRequestHandler<UpdateAlerteCommand>
{
    private readonly IAlerteRepository _alerteRepository;

    public UpdateAlerteCommandHandler(IAlerteRepository alerteRepository)
    {
        _alerteRepository = alerteRepository;
    }

    public async Task Handle(UpdateAlerteCommand request, CancellationToken cancellationToken)
    {
        var alerteId = new AlerteId(request.AlerteId);
        var typeId = new TypeId(request.TypeId);
        var dispositifId = new DeviceId(request.DispositifId);

        var alerte = Alerte.Create(
            alerteId,
            request.Date,
            dispositifId,
            typeId
        );

        await _alerteRepository.UpdateBulkAsync(alerte, cancellationToken)
            .ConfigureAwait(false);
    }
}
