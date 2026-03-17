namespace AutoRent.Messaging.Contracts;

public sealed record BookingPaymentConfirmed(
    int BookingId,
    Guid UserId,
    Guid PartnerUserId,
    int PartnerCarId,
    decimal? PriceHour,
    decimal? TotalPrice);

public sealed record BookingPaymentCanceled(int BookingId);

public sealed record BookingPaymentCompleted(int BookingId);
