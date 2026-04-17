namespace TicketService.Infrastructure.Events.Outbox;

internal sealed class PartnerBookingCancellationRejectedWorkflowPayload
{
    public Guid TicketId { get; init; }
    public PartnerBookingCancellationRejectedWorkflowStep CurrentStep { get; set; }
}

internal enum PartnerBookingCancellationRejectedWorkflowStep
{
    NotifyBookingService = 1,
    SendPartnerNotification = 2,
    Completed = 99
}
