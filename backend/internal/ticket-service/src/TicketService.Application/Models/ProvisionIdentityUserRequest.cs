namespace TicketService.Application.Models;

public sealed record ProvisionIdentityUserRequest(
    string FullName,
    string Email,
    DateOnly BirthDate,
    string? RequestKey = null,
    string SubjectType = "user",
    string ActorType = "client");
