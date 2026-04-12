namespace TicketService.Application.Models;

public sealed record IssueBookingCompletionFineWorkflowRequest(
    int BookingId,
    Guid TicketId,
    decimal? LatePenaltyAmount,
    decimal DamageFineAmount,
    string FineComment,
    string CustomerEmail,
    string CustomerFullName);
