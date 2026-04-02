namespace TicketService.Infrastructure.Events.Outbox;

internal sealed class BookingCompletionApprovedWorkflowPayload
{
    public Guid TicketId { get; init; }
    public BookingCompletionApprovedWorkflowStep CurrentStep { get; set; }
}

internal enum BookingCompletionApprovedWorkflowStep
{
    NotifyBookingService = 1,
    Completed = 99
}
