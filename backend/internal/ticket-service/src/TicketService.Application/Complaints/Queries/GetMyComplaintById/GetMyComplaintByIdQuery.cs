namespace TicketService.Application.Complaints.Queries.GetMyComplaintById;

public sealed record GetMyComplaintByIdQuery(Guid ComplaintId, Guid ReporterUserId);
