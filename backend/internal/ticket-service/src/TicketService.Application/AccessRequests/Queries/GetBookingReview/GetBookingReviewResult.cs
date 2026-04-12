using TicketService.Application.Models;

namespace TicketService.Application.AccessRequests.Queries.GetBookingReview;

public sealed record GetBookingReviewResult(BookingReviewDto Booking);
