namespace CarService.Application.DTOs.CarComment
{
    public class CarCommentUpdateDto
    {
        public required string Content { get; set; }
        public required int Rating { get; set; }
    }
}