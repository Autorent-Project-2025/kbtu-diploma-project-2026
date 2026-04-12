namespace TicketService.Api.Contracts.Tickets;

public sealed class IssueTicketFineRequest
{
    public decimal Amount { get; init; }
    public string Comment { get; init; } = string.Empty;
}
