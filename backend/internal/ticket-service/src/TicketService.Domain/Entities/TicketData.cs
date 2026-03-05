using System.Text.Json.Serialization;

namespace TicketService.Domain.Entities;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(ClientTicketData), typeDiscriminator: "client")]
[JsonDerivedType(typeof(PartnerTicketData), typeDiscriminator: "partner")]
[JsonDerivedType(typeof(PartnerCarTicketData), typeDiscriminator: "partner-car")]
public abstract record TicketData
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string? IdentityDocumentFileName { get; init; }
    public string? DecisionReason { get; init; }
    public Guid? ReviewedByManagerId { get; init; }
    public DateTime? ReviewedAt { get; init; }
}

public sealed record ClientTicketData : TicketData
{
    public DateOnly BirthDate { get; init; }
    public string? DriverLicenseFileName { get; init; }
    public string? AvatarUrl { get; init; }
}

public sealed record PartnerTicketData : TicketData
{
    public string CompanyName { get; init; } = string.Empty;
    public string ContactEmail { get; init; } = string.Empty;
}

public sealed record PartnerCarTicketData : TicketData
{
    public Guid RelatedPartnerUserId { get; init; }
    public string CarBrand { get; init; } = string.Empty;
    public string CarModel { get; init; } = string.Empty;
    public int? CarYear { get; init; }
    public string LicensePlate { get; init; } = string.Empty;
    public string OwnershipDocumentFileName { get; init; } = string.Empty;
    public IReadOnlyCollection<PartnerCarTicketImageData> CarImages { get; init; } = [];
}

public sealed record PartnerCarTicketImageData
{
    public string ImageId { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
}
