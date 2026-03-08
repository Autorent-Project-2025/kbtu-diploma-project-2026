namespace PartnerService.Application.Models;

public sealed record PartnerBookingResult(
    int Id,
    Guid UserId,
    int PartnerCarId,
    Guid PartnerUserId,
    string CarBrand,
    string CarModel,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    decimal? PriceHour,
    decimal? TotalPrice,
    DateTimeOffset CreatedAt,
    string? Status);
