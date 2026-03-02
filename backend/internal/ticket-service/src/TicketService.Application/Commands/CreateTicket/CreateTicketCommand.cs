namespace TicketService.Application.Commands.CreateTicket;

public sealed record CreateTicketCommand(string FullName, string Email, DateOnly BirthDate);
