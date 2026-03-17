namespace CarService.Application.DTOs.PartnerCars
{
    public class PartnerCarProvisionDto
    {
        public Guid RelatedUserId { get; set; }
        public string? ProvisionRequestKey { get; set; }
        public string CarBrand { get; set; } = string.Empty;
        public string CarModel { get; set; } = string.Empty;
        public int CarYear { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public decimal PriceHour { get; set; }
        public decimal PriceDay { get; set; }
        public string OwnershipFileName { get; set; } = string.Empty;
        public IReadOnlyCollection<PartnerCarProvisionImageDto> Images { get; set; } = [];
    }

    public class PartnerCarProvisionImageDto
    {
        public string ImageId { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }
}
