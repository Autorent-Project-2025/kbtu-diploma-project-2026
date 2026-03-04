namespace CarService.Api.Contracts.Internal
{
    public sealed class ProvisionPartnerCarRequest
    {
        public Guid RelatedUserId { get; init; }
        public string CarBrand { get; init; } = string.Empty;
        public string CarModel { get; init; } = string.Empty;
        public string LicensePlate { get; init; } = string.Empty;
        public string OwnershipDocumentFileName { get; init; } = string.Empty;
        public IReadOnlyCollection<ProvisionPartnerCarImageRequest> Images { get; init; } = [];
    }

    public sealed class ProvisionPartnerCarImageRequest
    {
        public string ImageId { get; init; } = string.Empty;
        public string ImageUrl { get; init; } = string.Empty;
    }
}
