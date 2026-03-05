using CarService.Domain.Enums;

namespace CarService.Application.DTOs.PartnerCars
{
    public class PartnerCarCreateDto
    {
        public int CarModelId { get; set; }
        public string LicensePlate { get; set; } = null!;
        public string? Color { get; set; }
        public decimal? PriceHour { get; set; }
        public decimal? PriceDay { get; set; }
        public PartnerCarStatus Status { get; set; } = PartnerCarStatus.Available;
    }
}
