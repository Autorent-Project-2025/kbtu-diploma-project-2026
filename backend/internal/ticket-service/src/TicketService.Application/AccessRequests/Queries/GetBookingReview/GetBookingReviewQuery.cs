namespace TicketService.Application.AccessRequests.Queries.GetBookingReview;

public sealed record GetBookingReviewQuery(
    Guid ComplaintId,
    Guid ManagerId,
    bool HasGlobalBookingView);
