namespace PartnerService.Api.Contracts.Partners;

public sealed class RequestPartnerPayoutRequest
{
    public decimal Amount { get; init; }
    public string RequestKey { get; init; } = string.Empty;
}
