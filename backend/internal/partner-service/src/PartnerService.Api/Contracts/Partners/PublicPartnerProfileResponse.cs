namespace PartnerService.Api.Contracts.Partners;

public sealed class PublicPartnerProfileResponse
{
    public string RelatedUserId { get; init; } = string.Empty;
    public string CarrierName { get; init; } = string.Empty;
}
