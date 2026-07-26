namespace CollectManagement.Application.Features.JoursFeries.Queries.GetOneJourFerie;

public record GetOneJourFerieResponse(
    Ulid JourFerieId,
    DateTime Date,
    string Label
);
