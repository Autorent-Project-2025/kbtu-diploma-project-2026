namespace TicketService.Api.Contracts.Complaints;

public sealed class RespondToInfoRequestRequest
{
    public string Message { get; set; } = string.Empty;
    public IReadOnlyCollection<IFormFile>? Attachments { get; set; }
}
