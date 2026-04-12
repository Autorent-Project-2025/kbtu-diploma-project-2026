namespace TicketService.Infrastructure.Events.Outbox;

internal sealed class PartnerBookingCancellationApprovedWorkflowPayload
{
    public Guid TicketId { get; init; }
    public PartnerBookingCancellationApprovedWorkflowStep CurrentStep { get; set; }
}

internal enum PartnerBookingCancellationApprovedWorkflowStep
{
    NotifyBookingService = 1,
    SendPartnerNotification = 2,
    Completed = 99
}
