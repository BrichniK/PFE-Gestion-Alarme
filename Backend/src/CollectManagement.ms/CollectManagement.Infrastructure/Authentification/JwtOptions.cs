namespace CollectManagement.Infrastructure.Authentification;

public class JwtOptions
{
    public static string SectionName { get; } = "JwtOptions";
    public string Secret { get; init; } = null!;
    public int ExpiryMinutes { get; init; }
    public string Issuer { get; init; } = null!;
    public string Audience { get; init; } = null!;
}