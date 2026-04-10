namespace CarService.Api.Contracts.Internal
{
    public sealed class ProvisionPartnerCarRequest
    {
        public Guid RelatedUserId { get; init; }
        public string? ProvisionRequestKey { get; init; }
        public string CarBrand { get; init; } = string.Empty;
        public string CarModel { get; init; } = string.Empty;
        public int CarYear { get; init; }
        public string LicensePlate { get; init; } = string.Empty;
        public string? Transmission { get; init; }
        public string? FuelType { get; init; }
        public int? Seats { get; init; }
        public int? Doors { get; init; }
        public string? BodyType { get; init; }
        public int? Horsepower { get; init; }
        public IReadOnlyCollection<string> SemanticTags { get; init; } = [];
        public string OwnershipDocumentFileName { get; init; } = string.Empty;
        public IReadOnlyCollection<ProvisionPartnerCarImageRequest> Images { get; init; } = [];
    }

    public sealed class ProvisionPartnerCarImageRequest
    {
        public string ImageId { get; init; } = string.Empty;
        public string ImageUrl { get; init; } = string.Empty;
    }
}
