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

/// <summary>
/// Asks payment-service to provision a mock payment session for a freshly
/// created booking. Replaces the synchronous HTTP call from booking-service
/// to payment-service that used to live on the booking-creation hot path.
/// Delivered via outbox + RabbitMQ so booking creation does not block on
/// payment-service availability.
/// </summary>
public sealed record BookingPaymentSessionRequested(
    int BookingId,
    Guid UserId,
    decimal TotalPrice,
    string Currency);
