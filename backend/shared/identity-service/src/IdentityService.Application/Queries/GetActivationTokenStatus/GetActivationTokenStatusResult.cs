namespace IdentityService.Application.Queries.GetActivationTokenStatus;

public sealed record GetActivationTokenStatusResult(
    bool IsValid,
    DateTime? ExpiresAtUtc,
    string? Reason);
