namespace CarService.Api.Contracts.Internal
{
    public sealed class ApplyApprovedPartnerCarUpdateRequest
    {
        public string LicensePlate { get; set; } = string.Empty;
        public string? Color { get; set; }
        public int? RequestedStatus { get; set; }
        public bool? IsActive { get; set; }
        public IReadOnlyCollection<ApplyApprovedPartnerCarUpdateImageRequest>? Images { get; set; }
    }

    public sealed class ApplyApprovedPartnerCarUpdateImageRequest
    {
        public string ImageId { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string ImageType { get; set; } = "general";
    }
}
