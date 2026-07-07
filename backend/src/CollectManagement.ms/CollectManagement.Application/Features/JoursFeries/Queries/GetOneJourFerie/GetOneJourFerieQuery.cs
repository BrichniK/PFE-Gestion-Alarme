namespace CollectManagement.Application.Features.JoursFeries.Queries.GetOneJourFerie;

public record GetOneJourFerieQuery(Ulid JourFerieId) : IRequest<GetOneJourFerieResponse>;
