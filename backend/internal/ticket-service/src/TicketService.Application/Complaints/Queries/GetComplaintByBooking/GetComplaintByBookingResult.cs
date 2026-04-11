using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Queries.GetComplaintByBooking;

public sealed record GetComplaintByBookingResult(ComplaintDto? Complaint);
