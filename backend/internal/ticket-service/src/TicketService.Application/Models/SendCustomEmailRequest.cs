namespace TicketService.Application.Models;

public sealed record SendCustomEmailRequest(
    string To,
    string Subject,
    string Text,
    string? Html = null);
