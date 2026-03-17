namespace IdentityService.Domain.Entities;

public sealed class UserProvisionRequest
{
    public Guid Id { get; set; }
    public string RequestKey { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public string SubjectType { get; set; } = string.Empty;
    public string ActorType { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
