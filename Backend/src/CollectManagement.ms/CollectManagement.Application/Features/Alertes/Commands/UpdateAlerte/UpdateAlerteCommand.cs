namespace CollectManagement.Application.Features.Alertes.Commands.UpdateAlerte;

public record UpdateAlerteCommand(
    Ulid AlerteId,
    DateTime? Date,
    Ulid DispositifId,
    Ulid TypeId
) : IRequest;
