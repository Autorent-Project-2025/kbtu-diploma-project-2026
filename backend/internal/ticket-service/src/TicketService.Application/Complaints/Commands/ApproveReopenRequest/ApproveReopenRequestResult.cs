using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Commands.ApproveReopenRequest;

public sealed record ApproveReopenRequestResult(
    ReopenRequestDto ReopenRequest,
    ComplaintDto Complaint);
