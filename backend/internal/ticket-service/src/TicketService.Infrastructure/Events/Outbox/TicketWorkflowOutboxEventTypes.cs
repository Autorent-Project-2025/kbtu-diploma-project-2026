namespace TicketService.Infrastructure.Events.Outbox;

internal static class TicketWorkflowOutboxEventTypes
{
    public const string Approved = "ticket.approved";
    public const string BookingCompletionApproved = "ticket.booking-completion.approved";
    public const string BookingCompletionFineIssued = "ticket.booking-completion.fine-issued";
    public const string PartnerBookingCancellationApproved = "ticket.partner-booking-cancellation.approved";
    public const string PartnerBookingCancellationRejected = "ticket.partner-booking-cancellation.rejected";
    public const string Rejected = "ticket.rejected";
}
