using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Commands.RespondToInfoRequest;

public sealed record RespondToInfoRequestResult(ComplaintDto Complaint);
