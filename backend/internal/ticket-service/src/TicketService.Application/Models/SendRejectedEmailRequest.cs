namespace TicketService.Application.Models;

public sealed record SendRejectedEmailRequest(
    string To,
    string FullName,
    string Reason);
