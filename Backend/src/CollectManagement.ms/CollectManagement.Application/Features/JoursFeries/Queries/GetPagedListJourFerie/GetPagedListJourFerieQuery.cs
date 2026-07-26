namespace CollectManagement.Application.Features.JoursFeries.Queries.GetPagedListJourFerie;

public record GetPagedListJourFerieQuery(
    string? Search,
    string? Sort,
    string? Order,
    int Page,
    int Size
) : IRequest<GetPagedListJourFerieResponse>;
