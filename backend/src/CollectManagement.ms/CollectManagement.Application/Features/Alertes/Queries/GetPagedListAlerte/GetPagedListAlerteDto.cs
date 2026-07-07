namespace CollectManagement.Application.Features.Alertes.Queries.GetPagedListAlerte;

public record GetPagedListAlerteDto(
    Ulid AlerteId,
    DateTime? Date,
    Ulid DispositifId,
    Ulid TypeId,
    string DispositifName,
    bool Traiter
);
