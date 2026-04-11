using TicketService.Domain.Enums;

namespace TicketService.Application.Complaints.Queries.GetAllComplaints;

public sealed record GetAllComplaintsQuery(
    ComplaintStatus? Status,
    ComplaintCategory? Category,
    ComplaintPriority? Priority,
    Guid? AssignedToManagerId);
