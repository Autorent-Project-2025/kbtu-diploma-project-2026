namespace TicketService.Api.Contracts.Tickets;

public sealed class CreateTicketRequest
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateOnly BirthDate { get; init; }
}
