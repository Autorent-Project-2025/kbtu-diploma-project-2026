using CarService.Application.DTOs.CarComment;
using CarService.Application.DTOs.CarImage;
using CarService.Application.DTOs.Bookings;

namespace CarService.Application.DTOs.PartnerCars
{
    public class PartnerCarDetailsDto : PartnerCarResponseDto
    {
        public List<CarImageDto> Images { get; set; } = [];
        public List<CarCommentResponseDto> Comments { get; set; } = [];
        public List<LinkedBookingDto> Bookings { get; set; } = [];
    }
}
