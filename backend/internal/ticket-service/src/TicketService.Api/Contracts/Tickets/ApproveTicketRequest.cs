namespace TicketService.Api.Contracts.Tickets;

public sealed class ApproveTicketRequest
{
    public PartnerCarTicketReviewDataRequest? PartnerCarData { get; init; }
}
