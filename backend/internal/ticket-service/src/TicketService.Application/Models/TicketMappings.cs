using TicketService.Domain.Entities;

namespace TicketService.Application.Models;

public static class TicketMappings
{
    public static TicketDto ToDto(this Ticket ticket)
    {
        return new TicketDto(
            ticket.Id,
            ticket.TicketType,
            ticket.Data,
            ticket.FirstName,
            ticket.LastName,
            ticket.FullName,
            ticket.Email,
            ticket.BirthDate,
            ticket.PhoneNumber,
            ticket.IdentityDocumentFileName,
            ticket.DriverLicenseFileName,
            ticket.OwnershipDocumentFileName,
            ticket.AvatarUrl,
            ticket.RelatedPartnerUserId,
            ticket.CarBrand,
            ticket.CarModel,
            ticket.LicensePlate,
            ticket.CarImages,
            ticket.Status,
            ticket.DecisionReason,
            ticket.CreatedAt,
            ticket.ReviewedByManagerId,
            ticket.ReviewedAt);
    }
}
