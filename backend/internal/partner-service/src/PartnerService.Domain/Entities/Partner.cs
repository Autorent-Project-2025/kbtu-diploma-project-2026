using System.ComponentModel.DataAnnotations.Schema;

namespace PartnerService.Domain.Entities;

public class Partner
{
    [Column("id")]
    public int Id { get; set; }

    [Column("owner_first_name")]
    public string OwnerFirstName { get; set; } = string.Empty;

    [Column("owner_last_name")]
    public string OwnerLastName { get; set; } = string.Empty;

    [Column("created_on")]
    public DateTime CreatedOn { get; set; }

    [Column("contract_file_name")]
    public string? ContractFileName { get; set; }

    [Column("owner_identity_file_name")]
    public string OwnerIdentityFileName { get; set; } = string.Empty;

    [Column("registration_date")]
    public DateOnly RegistrationDate { get; set; }

    [Column("partnership_end_date")]
    public DateOnly PartnershipEndDate { get; set; }

    [Column("related_user_id")]
    public string RelatedUserId { get; set; } = string.Empty;

    [Column("phone_number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Column("provision_request_key")]
    public string? ProvisionRequestKey { get; set; }
}
