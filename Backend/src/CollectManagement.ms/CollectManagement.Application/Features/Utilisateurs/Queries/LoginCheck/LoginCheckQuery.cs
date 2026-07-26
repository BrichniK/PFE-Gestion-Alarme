using CollectManagement.Application.Contracts.Authentication;

namespace CollectManagement.Application.Features.Utilisateurs.Queries.LoginCheck;

public record LoginCheckQuery(
    Ulid UtilisateurId
): IRequest<AuthenticationResponse>;