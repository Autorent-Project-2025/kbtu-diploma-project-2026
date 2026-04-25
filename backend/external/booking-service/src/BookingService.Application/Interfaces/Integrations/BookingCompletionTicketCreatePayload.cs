namespace BookingService.Application.Interfaces.Integrations;

public sealed class BookingCompletionTicketCreatePayload
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public int BookingId { get; init; }
    public DateTimeOffset PlannedStartTime { get; init; }
    public DateTimeOffset PlannedEndTime { get; init; }
    public DateTimeOffset TripStartedAt { get; init; }
    public DateTimeOffset TripCompletedAt { get; init; }
    public decimal? LatePenaltyAmount { get; init; }
    public FileUploadPayload CompletionFrontPhotoFile { get; init; } = new();
    public FileUploadPayload CompletionBackPhotoFile { get; init; } = new();
    public FileUploadPayload CompletionSideLeftPhotoFile { get; init; } = new();
    public FileUploadPayload CompletionSideRightPhotoFile { get; init; } = new();
    public FileUploadPayload CompletionInteriorPhotoFile { get; init; } = new();

    // AI damage assessment. Advisory-only — never fed into
    // damageFineAmount. The manager reviews it alongside the raw photos
    // and still decides manually whether to issue a fine. Nullable
    // because a fail-open path can create a ticket without an assessment.
    public DamageAssessmentPayload? DamageAssessment { get; init; }
}

/// <summary>
/// Serializable form of the AI assessment that booking-service forwards
/// to ticket-service. Kept flat so it rides easily inside a multipart
/// form as a JSON string.
/// </summary>
public sealed class DamageAssessmentPayload
{
    public string Status { get; init; } = string.Empty;
    public string? Verdict { get; init; }
    public int ValidPhotosCount { get; init; }
    public DateTimeOffset ProcessedAtUtc { get; init; }
    public string? ErrorMessage { get; init; }
    public IReadOnlyList<DamageAssessmentDamagePayload> Damages { get; init; } = Array.Empty<DamageAssessmentDamagePayload>();
    public IReadOnlyList<DamageAssessmentRejectedPhotoPayload> RejectedPhotos { get; init; } = Array.Empty<DamageAssessmentRejectedPhotoPayload>();
}

public sealed class DamageAssessmentDamagePayload
{
    public string Type { get; init; } = string.Empty;
    public double Confidence { get; init; }
    public IReadOnlyList<int> BoundingBox { get; init; } = Array.Empty<int>();
    public string? Slot { get; init; }
    public string? SourceFile { get; init; }
}

public sealed class DamageAssessmentRejectedPhotoPayload
{
    public string? Slot { get; init; }
    public string FileName { get; init; } = string.Empty;
    public int Step { get; init; }
    public string Reason { get; init; } = string.Empty;
    public IReadOnlyList<string> Details { get; init; } = Array.Empty<string>();
}
