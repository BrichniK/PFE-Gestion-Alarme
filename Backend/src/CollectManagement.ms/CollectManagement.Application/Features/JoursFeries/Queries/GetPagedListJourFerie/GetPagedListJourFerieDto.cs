namespace CollectManagement.Application.Features.JoursFeries.Queries.GetPagedListJourFerie;

public record GetPagedListJourFerieDto(
    Ulid JourFerieId,
    DateTime Date,
    string Label
);
