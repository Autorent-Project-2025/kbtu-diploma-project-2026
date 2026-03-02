namespace CarService.Application.DTOs.CarComment
{
    public class CarCommentCreateDto
    {
        public int CarId { get; set; }
        public required string Content { get; set; }
        public required int Rating { get; set; }
    }
}
