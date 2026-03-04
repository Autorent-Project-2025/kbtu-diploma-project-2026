using TicketService.Application.Models;

namespace TicketService.Application.Commands.RejectTicket;

public sealed record RejectTicketCommand(
    Guid TicketId,
    Guid ManagerId,
    string DecisionReason,
    PartnerCarTicketReviewData? PartnerCarData);
