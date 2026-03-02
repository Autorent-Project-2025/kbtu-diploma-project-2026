namespace TicketService.Application.Models;

public sealed record ProvisionIdentityUserRequest(string FullName, string Email, DateOnly BirthDate);
