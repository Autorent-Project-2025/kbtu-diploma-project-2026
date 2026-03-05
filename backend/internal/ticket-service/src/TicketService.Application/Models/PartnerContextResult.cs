namespace TicketService.Application.Models;

public sealed record PartnerContextResult(
    string OwnerFirstName,
    string OwnerLastName,
    string PhoneNumber,
    Guid RelatedUserId);
