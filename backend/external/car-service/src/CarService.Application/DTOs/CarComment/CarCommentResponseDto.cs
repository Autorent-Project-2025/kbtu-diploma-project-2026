namespace CarService.Application.DTOs.CarComment
{
    public class CarCommentResponseDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int CarId { get; set; }
        public int? PartnerCarId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
