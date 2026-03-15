using System.ComponentModel.DataAnnotations;

namespace ClientService.Api.Contracts.Profile;

public sealed class UpdateProfileRequest
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    public DateOnly BirthDate { get; set; }

    [Required]
    [MaxLength(32)]
    public string PhoneNumber { get; set; } = string.Empty;

    [MaxLength(1024)]
    public string? AvatarUrl { get; set; }
}
