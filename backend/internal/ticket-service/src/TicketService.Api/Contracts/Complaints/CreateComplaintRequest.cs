namespace TicketService.Api.Contracts.Complaints;

public sealed class CreateComplaintRequest
{
    public int BookingId { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IReadOnlyCollection<IFormFile>? Attachments { get; set; }
}
