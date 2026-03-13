namespace ClientService.Application.DTOs;

public sealed class ProfileUpdateDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}
