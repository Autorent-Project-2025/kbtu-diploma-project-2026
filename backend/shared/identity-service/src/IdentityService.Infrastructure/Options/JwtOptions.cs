namespace IdentityService.Infrastructure.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string PrivateKey { get; init; } = string.Empty;
    public string? PublicKey { get; init; }
    public string Issuer { get; init; } = "autorent-identity-service";
    public string Audience { get; init; } = "autorent-service";
    public int AccessTokenLifetimeMinutes { get; init; } = 15;
    public int RefreshTokenLifetimeDays { get; init; } = 30;
    public string KeyId { get; init; } = "identity-main";
}
