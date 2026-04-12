using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Queries.GetComplaintById;

public sealed record GetComplaintByIdResult(ComplaintDto Complaint);
