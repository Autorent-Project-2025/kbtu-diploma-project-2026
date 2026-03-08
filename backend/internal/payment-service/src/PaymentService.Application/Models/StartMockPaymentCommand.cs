namespace PaymentService.Application.Models;

public sealed record StartMockPaymentCommand(
    int BookingId,
    Guid UserId,
    decimal Amount,
    string Currency);
