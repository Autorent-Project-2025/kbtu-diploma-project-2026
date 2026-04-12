namespace TicketService.Application.Models;

public sealed record BookingReviewDto(
    int BookingId,
    string Status,
    string CarBrand,
    string CarModel,
    string? CoverImageUrl,
    string? PartnerName,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    decimal? TotalPrice,
    DateTimeOffset? TripStartedAt,
    Guid ComplaintId,
    string ComplaintSubject);
