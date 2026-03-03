using TicketService.Domain.Entities;

namespace TicketService.Application.Models;

public static class TicketMappings
{
    public static TicketDto ToDto(this Ticket ticket)
    {
        return new TicketDto(
            ticket.Id,
            ticket.FullName,
            ticket.Email,
            ticket.BirthDate,
            ticket.PhoneNumber,
            ticket.IdentityDocumentFileName,
            ticket.DriverLicenseFileName,
            ticket.AvatarUrl,
            ticket.Status,
            ticket.DecisionReason,
            ticket.CreatedAt,
            ticket.ReviewedByManagerId,
            ticket.ReviewedAt);
    }
}
