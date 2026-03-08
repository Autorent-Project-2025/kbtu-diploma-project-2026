namespace PaymentService.Application.Models;

public sealed record BookingPaymentSnapshot(
    int BookingId,
    Guid UserId,
    Guid PartnerUserId,
    int PartnerCarId,
    decimal? PriceHour,
    decimal? TotalPrice);
