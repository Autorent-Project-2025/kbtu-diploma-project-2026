namespace TicketService.Application.Complaints.Queries.GetComplaintByBooking;

public sealed record GetComplaintByBookingQuery(
    int BookingId,
    Guid ReporterUserId);
