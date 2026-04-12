using CarService.Domain.Enums;

namespace CarService.Application.DTOs.PartnerCars
{
    public sealed class PartnerCarApprovedUpdateDto
    {
        public string LicensePlate { get; set; } = string.Empty;
        public string? Color { get; set; }
        public PartnerCarStatus? RequestedStatus { get; set; }
        public bool? IsActive { get; set; }
        public IReadOnlyCollection<PartnerCarApprovedUpdateImageDto> Images { get; set; } = [];
    }

    public sealed class PartnerCarApprovedUpdateImageDto
    {
        public string ImageId { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string ImageType { get; set; } = "general";
    }
}
