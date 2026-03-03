namespace ClientService.Application.DTOs;

public sealed class ClientResponseDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public DateOnly BirthDate { get; set; }
    public string? IdentityDocumentFileName { get; set; }
    public string? DriverLicenseFileName { get; set; }
    public string RelatedUserId { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}
