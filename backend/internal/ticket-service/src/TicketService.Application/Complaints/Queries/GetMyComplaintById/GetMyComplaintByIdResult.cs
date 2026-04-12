using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Queries.GetMyComplaintById;

public sealed record GetMyComplaintByIdResult(ComplaintDto Complaint);
