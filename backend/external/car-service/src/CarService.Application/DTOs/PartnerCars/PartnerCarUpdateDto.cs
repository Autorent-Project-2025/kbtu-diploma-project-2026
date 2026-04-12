using CarService.Domain.Enums;

namespace CarService.Application.DTOs.PartnerCars
{
    public class PartnerCarUpdateDto
    {
        public string LicensePlate { get; set; } = null!;
        public string? Color { get; set; }
        public PartnerCarStatus Status { get; set; } = PartnerCarStatus.Available;
    }
}
