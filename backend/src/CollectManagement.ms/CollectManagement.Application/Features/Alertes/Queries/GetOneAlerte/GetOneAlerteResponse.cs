namespace CollectManagement.Application.Features.Alertes.Queries.GetOneAlerte;

public record GetOneAlerteResponse(
    Ulid AlerteId,
    DateTime? Date,
    Ulid DispositifId,
    Ulid TypeId,
    bool Traiter
);
