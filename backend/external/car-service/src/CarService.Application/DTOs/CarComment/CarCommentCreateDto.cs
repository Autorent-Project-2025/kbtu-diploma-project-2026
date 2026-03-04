namespace CarService.Application.DTOs.CarComment
{
    public class CarCommentCreateDto
    {
        public int PartnerCarId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
    }
}
