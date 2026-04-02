namespace BookingService.Application.Interfaces.Integrations;

public sealed class IdentityUserPayload
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string SubjectType { get; set; } = string.Empty;
    public string ActorType { get; set; } = string.Empty;
}
