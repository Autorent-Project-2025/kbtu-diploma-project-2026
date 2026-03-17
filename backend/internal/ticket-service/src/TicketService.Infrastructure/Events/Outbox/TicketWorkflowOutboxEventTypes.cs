namespace TicketService.Infrastructure.Events.Outbox;

internal static class TicketWorkflowOutboxEventTypes
{
    public const string Approved = "ticket.approved";
    public const string Rejected = "ticket.rejected";
}
