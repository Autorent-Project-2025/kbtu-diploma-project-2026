namespace TicketService.Application.Models;

public sealed record ProvisionIdentityUserResult(
    Guid UserId,
    string Username,
    string Email,
    string ActivationToken,
    DateTime ActivationExpiresAtUtc);
