namespace CarService.Application.DTOs.PartnerCars
{
    public sealed class PartnerCarSnapshotDto
    {
        public int PartnerCarId { get; set; }
        public Guid PartnerUserId { get; set; }
        public string CarBrand { get; set; } = string.Empty;
        public string CarModel { get; set; } = string.Empty;
        public int ModelYear { get; set; }
        public string? LicensePlate { get; set; }
        public decimal? PriceHour { get; set; }
        public decimal? Rating { get; set; }
        public string? CoverImageUrl { get; set; }
        public IReadOnlyList<string> ImageUrls { get; set; } = [];
    }
}
