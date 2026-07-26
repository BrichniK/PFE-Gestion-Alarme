namespace CollectManagement.Application.Features.Types.Queries.GetOneType;

public record GetOneTypeResponse(
    Ulid TypeId,
    string Code,
    string Label,
    int? DureeNominal
);
