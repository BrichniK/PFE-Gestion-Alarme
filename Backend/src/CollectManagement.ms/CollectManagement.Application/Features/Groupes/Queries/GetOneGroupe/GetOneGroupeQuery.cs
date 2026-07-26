namespace CollectManagement.Application.Features.Groupes.Queries.GetOneGroupe;

public record GetOneGroupeQuery(Ulid GroupeId) : IRequest<GetOneGroupeResponse>;
