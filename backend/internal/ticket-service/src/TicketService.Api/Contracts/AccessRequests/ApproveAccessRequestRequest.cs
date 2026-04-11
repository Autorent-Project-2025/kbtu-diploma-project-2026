namespace TicketService.Api.Contracts.AccessRequests;

public sealed class ApproveAccessRequestRequest
{
    public string? DecisionNote { get; set; }
    public int ExpiresInHours { get; set; } = 24;
}
