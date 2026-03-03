namespace TicketService.Application.Models;

public sealed record SendPartnerRejectedEmailRequest(
    string To,
    string FullName,
    string Reason);
