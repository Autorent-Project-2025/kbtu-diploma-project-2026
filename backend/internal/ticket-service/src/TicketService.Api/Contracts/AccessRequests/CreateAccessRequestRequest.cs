namespace TicketService.Api.Contracts.AccessRequests;

public sealed class CreateAccessRequestRequest
{
    public string Reason { get; set; } = string.Empty;
}
