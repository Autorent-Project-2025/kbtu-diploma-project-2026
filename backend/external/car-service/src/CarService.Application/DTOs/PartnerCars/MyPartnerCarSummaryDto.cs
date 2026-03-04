namespace CarService.Application.DTOs.PartnerCars
{
    public class MyPartnerCarSummaryDto
    {
        public int Id { get; set; }
        public string ModelDisplayName { get; set; } = string.Empty;
        public decimal? Rating { get; set; }
        public int BookingCount { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string? OwnershipFileName { get; set; }
        public decimal? PriceHour { get; set; }
        public decimal? PriceDay { get; set; }
        public string? Color { get; set; }
    }
}
