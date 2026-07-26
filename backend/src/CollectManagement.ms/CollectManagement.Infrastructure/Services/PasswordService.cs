using System.Security.Cryptography;
using CollectManagement.Application.Common;
using CollectManagement.Application.Interfaces.Services;
using CollectManagement.Domain.Utilisateurs.ValueObjects;
using PasswordGenerator;

namespace CollectManagement.Infrastructure.Services;

public class PasswordService : IPasswordService
{
    public string HashPassword(UtilisateurId utilisateurId, string password)
    {
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            utilisateurId.Value.ToByteArray(),
            350000,
            HashAlgorithmName.SHA512,
            64);
        return Convert.ToHexString(hash);
    }

    public string GeneratePassword()
    {
        var pwd = new Password(8).IncludeLowercase().IncludeUppercase().IncludeSpecial();
        return pwd.Next();
    }

    public PasswordVerificationResult
        VerifyHashedPassword(UtilisateurId utilisateurId, string hashedPassword, string providedPassword)
    {
        var hashProvided = HashPassword(utilisateurId, providedPassword);
        
        return hashedPassword == hashProvided 
            ? PasswordVerificationResult.Succes 
            : PasswordVerificationResult.Failure ;
    }
}