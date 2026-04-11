using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Queries.GetAllComplaints;

public sealed record GetAllComplaintsResult(IReadOnlyCollection<ComplaintDto> Complaints);
