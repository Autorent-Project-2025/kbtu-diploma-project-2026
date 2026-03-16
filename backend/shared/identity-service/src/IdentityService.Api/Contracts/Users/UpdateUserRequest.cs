namespace IdentityService.Api.Contracts.Users;

public sealed class UpdateUserRequest
{
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? SubjectType { get; init; }
    public string? ActorType { get; init; }
}
