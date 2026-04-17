using System.Text.Json.Serialization;

namespace TicketService.Domain.Entities;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(ClientTicketData), typeDiscriminator: "client")]
[JsonDerivedType(typeof(PartnerTicketData), typeDiscriminator: "partner")]
[JsonDerivedType(typeof(PartnerCarTicketData), typeDiscriminator: "partner-car")]
[JsonDerivedType(typeof(BookingCompletionTicketData), typeDiscriminator: "booking-completion")]
[JsonDerivedType(typeof(PartnerBookingCancellationTicketData), typeDiscriminator: "partner-booking-cancellation")]
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
    public string? Transmission { get; init; }
    public string? FuelType { get; init; }
    public int? Seats { get; init; }
    public int? Doors { get; init; }
    public string? BodyType { get; init; }
    public int? Horsepower { get; init; }
    public IReadOnlyCollection<string> SelectedTags { get; init; } = [];
    public IReadOnlyCollection<string> SuggestedTags { get; init; } = [];
    public IReadOnlyCollection<string> ConfirmedTags { get; init; } = [];
    public string OwnershipDocumentFileName { get; init; } = string.Empty;
    public IReadOnlyCollection<PartnerCarTicketImageData> CarImages { get; init; } = [];
}

public sealed record PartnerCarTicketImageData
{
    public string ImageId { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public string ImageType { get; init; } = "general";
}

public sealed record BookingCompletionTicketData : TicketData
{
    public int BookingId { get; init; }
    public DateTimeOffset PlannedStartTime { get; init; }
    public DateTimeOffset PlannedEndTime { get; init; }
    public DateTimeOffset TripStartedAt { get; init; }
    public DateTimeOffset TripCompletedAt { get; init; }
    public decimal? LatePenaltyAmount { get; init; }
    public decimal? DamageFineAmount { get; init; }
    public IReadOnlyCollection<BookingCompletionTicketPhotoData> CompletionPhotos { get; init; } = [];
}

public sealed record PartnerBookingCancellationTicketData : TicketData
{
    public int BookingId { get; init; }
    public Guid RelatedPartnerUserId { get; init; }
    public string CarBrand { get; init; } = string.Empty;
    public string CarModel { get; init; } = string.Empty;
    public string BookingStatus { get; init; } = string.Empty;
    public DateTimeOffset BookingStartTime { get; init; }
    public DateTimeOffset BookingEndTime { get; init; }
    public string PartnerReason { get; init; } = string.Empty;
}

public sealed record BookingCompletionTicketPhotoData
{
    public string Slot { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
}
