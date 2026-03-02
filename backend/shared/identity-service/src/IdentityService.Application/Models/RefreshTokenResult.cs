namespace IdentityService.Application.Models;

public sealed record RefreshTokenResult(string Token, DateTime ExpiresAtUtc);
