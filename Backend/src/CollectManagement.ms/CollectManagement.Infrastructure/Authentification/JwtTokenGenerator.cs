using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CollectManagement.Application.Interfaces.Authentification;
using CollectManagement.Application.Interfaces.Services;
using CollectManagement.Domain.Utilisateurs;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CollectManagement.Infrastructure.Authentification;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly JwtOptions _jwtOptions;

    public JwtTokenGenerator(
        IDateTimeProvider dateTimeProvider, 
        IOptions<JwtOptions> jwtOptions)
    {
        _dateTimeProvider = dateTimeProvider;
        _jwtOptions = jwtOptions.Value;
    }
    
    public string GenerateToken(Utilisateur utilisateur)
    {
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtOptions.Secret)),
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, utilisateur.UtilisateurId.Value.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, utilisateur.NomUtilisateur),
            new Claim(JwtRegisteredClaimNames.Email, utilisateur.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Ulid.NewUlid().ToString()),
            new Claim(ClaimTypes.Role, utilisateur.RoleUtilisateurId?.Value.ToString()??"")
        };

        var securityToken = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            expires: _dateTimeProvider.Now.AddMinutes(_jwtOptions.ExpiryMinutes),
            claims: claims,
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(securityToken);
    }
}