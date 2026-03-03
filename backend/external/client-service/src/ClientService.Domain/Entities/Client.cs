using System.ComponentModel.DataAnnotations.Schema;

namespace ClientService.Domain.Entities;

public class Client
{
    [Column("id")]
    public int Id { get; set; }

    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;

    [Column("created_on")]
    public DateTime CreatedOn { get; set; }

    [Column("birth_date")]
    public DateOnly BirthDate { get; set; }

    [Column("identity_document_file_name")]
    public string? IdentityDocumentFileName { get; set; }

    [Column("driver_license_file_name")]
    public string? DriverLicenseFileName { get; set; }

    [Column("related_user_id")]
    public string RelatedUserId { get; set; } = string.Empty;

    [Column("phone_number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Column("avatar_url")]
    public string? AvatarUrl { get; set; }
}
