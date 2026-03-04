namespace CarService.Application.DTOs.CarComment
{
    public class CarCommentUpdateDto
    {
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
    }
}
