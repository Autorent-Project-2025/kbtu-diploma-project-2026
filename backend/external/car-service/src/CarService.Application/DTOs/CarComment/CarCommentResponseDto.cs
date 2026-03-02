namespace CarService.Application.DTOs.CarComment
{
    public class CarCommentResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CarId { get; set; }
        public required string Content { get; set; }
        public required int Rating { get; set; }
        public DateTime Created_On { get; set; }
    }
}
