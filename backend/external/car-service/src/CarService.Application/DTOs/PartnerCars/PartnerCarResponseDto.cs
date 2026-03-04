using CarService.Domain.Enums;

namespace CarService.Application.DTOs.PartnerCars
{
    public class PartnerCarResponseDto
    {
        public int Id { get; set; }
        public Guid PartnerId { get; set; }
        public int CarModelId { get; set; }
        public string LicensePlate { get; set; } = null!;
        public string? Color { get; set; }
        public decimal? PriceHour { get; set; }
        public decimal? PriceDay { get; set; }
        public PartnerCarStatus Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public decimal? Rating { get; set; }
        public int RatingsCount { get; set; }

        public string ModelBrand { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public int ModelYear { get; set; }
    }
}
