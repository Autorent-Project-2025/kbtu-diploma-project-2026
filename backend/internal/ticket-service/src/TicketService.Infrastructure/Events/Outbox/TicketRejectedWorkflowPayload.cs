namespace TicketService.Infrastructure.Events.Outbox;

internal sealed class TicketRejectedWorkflowPayload
{
    public Guid TicketId { get; init; }
    public TicketRejectedWorkflowStep CurrentStep { get; set; }
}

internal enum TicketRejectedWorkflowStep
{
    PublishRejectedEmail = 1,
    Completed = 99
}
