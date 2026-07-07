namespace CollectManagement.Application.Features.JoursFeries.Queries.GetPagedListJourFerie;

public record GetPagedListJourFerieResponse(
    List<GetPagedListJourFerieDto> JoursFeries,
    int Length
);
