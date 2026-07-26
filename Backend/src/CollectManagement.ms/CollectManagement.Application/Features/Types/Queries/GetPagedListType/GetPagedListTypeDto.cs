namespace CollectManagement.Application.Features.Types.Queries.GetPagedListType;

public record GetPagedListTypeDto(
    Ulid TypeId,
    string Code,
    string Label,
    int? DureeNominal
);
