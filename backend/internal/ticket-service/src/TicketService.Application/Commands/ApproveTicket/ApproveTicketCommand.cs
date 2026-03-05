using TicketService.Application.Models;

namespace TicketService.Application.Commands.ApproveTicket;

public sealed record ApproveTicketCommand(
    Guid TicketId,
    Guid ManagerId,
    PartnerCarTicketReviewData? PartnerCarData);
