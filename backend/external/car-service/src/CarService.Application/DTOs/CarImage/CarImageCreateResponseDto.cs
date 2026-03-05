using CarService.Domain.Enums;

namespace CarService.Application.DTOs.CarImage
{
    public class CarImageCreateResponseDto
    {
        public int Id { get; set; }
        public int? ModelId { get; set; }
        public int? PartnerCarId { get; set; }
        public string? ImageId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public CarImageType ImageType { get; set; }
        public int DisplayOrder { get; set; }
    }
}
