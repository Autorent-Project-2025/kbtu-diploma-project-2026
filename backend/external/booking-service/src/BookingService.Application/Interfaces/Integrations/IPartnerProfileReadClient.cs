namespace BookingService.Application.Interfaces.Integrations
{
    public interface IPartnerProfileReadClient
    {
        Task<PartnerPublicProfilePayload?> GetPublicProfileByRelatedUserIdAsync(
            Guid relatedUserId,
            CancellationToken cancellationToken = default);
    }

    public sealed class PartnerPublicProfilePayload
    {
        public Guid RelatedUserId { get; set; }
        public string CarrierName { get; set; } = string.Empty;
        public string OwnerFirstName { get; set; } = string.Empty;
        public string OwnerLastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
