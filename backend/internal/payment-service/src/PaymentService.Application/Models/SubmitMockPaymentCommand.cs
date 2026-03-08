namespace PaymentService.Application.Models;

public sealed record SubmitMockPaymentCommand(
    int BookingId,
    Guid UserId,
    string SessionKey,
    string CardHolder,
    string CardNumber,
    int ExpiryMonth,
    int ExpiryYear,
    string Cvv);
