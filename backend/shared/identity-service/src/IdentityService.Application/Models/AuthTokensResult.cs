namespace IdentityService.Application.Models;

public sealed record AuthTokensResult(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);
