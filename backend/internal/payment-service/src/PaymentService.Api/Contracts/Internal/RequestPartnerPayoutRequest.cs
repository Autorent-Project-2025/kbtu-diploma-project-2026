namespace PaymentService.Api.Contracts.Internal;

public sealed class RequestPartnerPayoutRequest
{
    public Guid PartnerUserId { get; init; }
    public decimal Amount { get; init; }
    public string RequestKey { get; init; } = string.Empty;
}
