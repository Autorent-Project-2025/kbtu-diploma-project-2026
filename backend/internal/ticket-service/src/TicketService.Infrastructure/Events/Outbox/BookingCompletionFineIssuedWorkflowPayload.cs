namespace TicketService.Infrastructure.Events.Outbox;

internal sealed class BookingCompletionFineIssuedWorkflowPayload
{
    public Guid TicketId { get; init; }
    public BookingCompletionFineIssuedWorkflowStep CurrentStep { get; set; }
}

internal enum BookingCompletionFineIssuedWorkflowStep
{
    NotifyBookingService = 1,
    Completed = 99
}
