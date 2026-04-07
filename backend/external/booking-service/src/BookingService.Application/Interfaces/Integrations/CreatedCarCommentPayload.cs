namespace BookingService.Application.Interfaces.Integrations;

public sealed class CreatedCarCommentPayload
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public DateTime CreatedOn { get; set; }
}
