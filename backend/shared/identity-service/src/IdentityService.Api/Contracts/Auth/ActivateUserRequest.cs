namespace IdentityService.Api.Contracts.Auth;

public sealed class ActivateUserRequest
{
    public string ActivationToken { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
