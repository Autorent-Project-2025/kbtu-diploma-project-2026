namespace TicketService.Api.Contracts.AccessRequests;

public sealed class RejectAccessRequestRequest
{
    public string? DecisionNote { get; set; }
}
