using CollectManagement.Domain.Utilisateurs;

namespace CollectManagement.Application.Interfaces.Authentification;

public interface IJwtTokenGenerator
{
    string GenerateToken(Utilisateur utilisateur);
}