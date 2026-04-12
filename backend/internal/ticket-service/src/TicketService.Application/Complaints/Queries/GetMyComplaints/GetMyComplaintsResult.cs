using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Queries.GetMyComplaints;

public sealed record GetMyComplaintsResult(IReadOnlyCollection<ComplaintDto> Complaints);
