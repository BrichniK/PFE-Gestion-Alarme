using CollectManagement.Application.Common;
using CollectManagement.Domain.Utilisateurs.ValueObjects;

namespace CollectManagement.Application.Interfaces.Services;

public interface IPasswordService
{
    string HashPassword(UtilisateurId utilisateurId, string password);
    
    string GeneratePassword();

    PasswordVerificationResult VerifyHashedPassword(
        UtilisateurId utilisateurId, string hashedPassword, string providedPassword);
}