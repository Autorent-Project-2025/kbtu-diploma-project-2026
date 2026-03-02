namespace TicketService.Application.Models;

public sealed record SendApprovedEmailRequest(
    string To,
    string FullName,
    string LoginEmail,
    string SetPasswordUrl);
