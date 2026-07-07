using CollectManagement.Application.Interfaces.Repositories.Alertes;
using CollectManagement.Domain.Alertes;
using CollectManagement.Domain.Alertes.ValueObjects;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Types.ValueObjects;

namespace CollectManagement.Application.Features.Alertes.Commands.CreateAlerte;

public class CreateAlerteCommandHandler
    : IRequestHandler<CreateAlerteCommand, CreateAlerteResponse>
{
    private readonly IAlerteRepository _alerteRepository;
    private readonly IMapper _mapper;

    public CreateAlerteCommandHandler(
        IAlerteRepository alerteRepository,
        IMapper mapper)
    {
        _alerteRepository = alerteRepository;
        _mapper = mapper;
    }

    public async Task<CreateAlerteResponse> Handle(CreateAlerteCommand request, CancellationToken cancellationToken)
    {
        var alerteId = new AlerteId(Ulid.NewUlid());
        var typeId = new TypeId(request.TypeId);
        var dispositifId = new DeviceId(request.DispositifId);

        var alerte = Alerte.Create(
            alerteId,
            request.Date,
            dispositifId,
            typeId
        );

        await _alerteRepository
            .AddAsync(alerte, cancellationToken)
            .ConfigureAwait(false);

        return _mapper.Map<CreateAlerteResponse>(alerte);
    }
}
