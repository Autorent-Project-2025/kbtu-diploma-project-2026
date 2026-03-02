namespace IdentityService.Application.Models;

public sealed record AccessTokenResult(string Token, DateTime ExpiresAtUtc);
