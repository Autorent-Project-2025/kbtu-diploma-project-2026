namespace PaymentService.Api.Contracts.Internal;

public sealed class PayoutFailureRequest
{
    public string? Reason { get; init; }
}
