namespace ClientService.Application.DTOs;

public sealed class MeResponseDto
{
    public string UserId { get; init; } = string.Empty;
    public string? Username { get; init; }
    public string[] Permissions { get; init; } = [];
    public string[] Roles { get; init; } = [];
    public DateTimeOffset? IssuedAtUtc { get; init; }
    public DateTimeOffset? ExpiresAtUtc { get; init; }
}
