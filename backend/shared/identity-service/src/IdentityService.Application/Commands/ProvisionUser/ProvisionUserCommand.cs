namespace IdentityService.Application.Commands.ProvisionUser;

public sealed record ProvisionUserCommand(
    string FullName,
    string Email,
    DateOnly BirthDate,
    string? RequestKey,
    string SubjectType,
    string ActorType);
