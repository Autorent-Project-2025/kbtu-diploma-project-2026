namespace TicketService.Api.Contracts.Tickets;

public sealed class RejectTicketRequest
{
    public string DecisionReason { get; init; } = string.Empty;
    public PartnerCarTicketReviewDataRequest? PartnerCarData { get; init; }
}
