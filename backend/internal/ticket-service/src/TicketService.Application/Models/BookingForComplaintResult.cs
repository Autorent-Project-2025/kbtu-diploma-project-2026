namespace TicketService.Application.Models;

public sealed record BookingForComplaintResult(
    int Id,
    Guid UserId,
    Guid PartnerUserId,
    string Status,
    string CarBrand,
    string CarModel,
    string? PartnerName,
    string? CoverImageUrl,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    decimal? TotalPrice,
    DateTimeOffset? TripStartedAt);
