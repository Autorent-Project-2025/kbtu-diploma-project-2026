using TicketService.Application.Models;

namespace TicketService.Application.Complaints.Commands.RequestInfo;

public sealed record RequestInfoResult(ComplaintDto Complaint);
