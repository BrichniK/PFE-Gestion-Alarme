using CollectManagement.Application.Contracts.Authentication;

namespace CollectManagement.Application.Features.Utilisateurs.Queries.Login;

public record LoginQuery(
    string Login,
    string Password
): IRequest<AuthenticationResponse>;