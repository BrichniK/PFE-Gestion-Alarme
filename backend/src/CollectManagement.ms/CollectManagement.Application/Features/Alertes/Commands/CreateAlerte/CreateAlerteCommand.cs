namespace CollectManagement.Application.Features.Alertes.Commands.CreateAlerte;

public record CreateAlerteCommand(
    DateTime? Date,
    Ulid DispositifId,
    Ulid TypeId
) : IRequest<CreateAlerteResponse>;
