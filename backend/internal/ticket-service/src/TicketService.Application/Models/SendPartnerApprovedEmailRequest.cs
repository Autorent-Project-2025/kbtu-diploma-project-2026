namespace TicketService.Application.Models;

public sealed record SendPartnerApprovedEmailRequest(
    string To,
    string FullName,
    string LoginEmail,
    string SetPasswordUrl);
