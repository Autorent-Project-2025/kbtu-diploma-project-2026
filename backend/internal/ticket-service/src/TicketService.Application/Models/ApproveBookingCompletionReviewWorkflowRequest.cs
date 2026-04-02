namespace TicketService.Application.Models;

public sealed record ApproveBookingCompletionReviewWorkflowRequest(
    int BookingId,
    Guid TicketId,
    decimal? LatePenaltyAmount,
    string CustomerEmail,
    string CustomerFullName);
