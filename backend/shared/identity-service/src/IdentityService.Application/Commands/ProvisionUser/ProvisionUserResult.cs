namespace IdentityService.Application.Commands.ProvisionUser;

public sealed record ProvisionUserResult(
    Guid UserId,
    string Username,
    string Email,
    string ActivationToken,
    DateTime ActivationExpiresAtUtc);
