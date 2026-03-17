namespace IdentityService.Api.Contracts.Internal;

public sealed class ProvisionUserRequest
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateOnly BirthDate { get; init; }
    public string? RequestKey { get; init; }
    public string SubjectType { get; init; } = "user";
    public string ActorType { get; init; } = "client";
}
