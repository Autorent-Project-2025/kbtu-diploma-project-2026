namespace CarService.Application.DTOs.CarComment
{
    public class CarCommentCreateDto
    {
        public int BookingId { get; set; }
        public int PartnerCarId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
    }
}
