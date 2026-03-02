using CarService.Domain.Enums;

namespace CarService.Application.DTOs.CarImage
{
    public class CarImageCreateResponseDto
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public required string ImageUrl { get; set; }
        public required CarImageType ImageType { get; set; }
        public required int DisplayOrder { get; set; }
    }
}
