namespace TicketService.Application.Models;

public sealed record SendPartnerCarApprovedEmailRequest(
    string To,
    string FullName,
    string CarBrand,
    string CarModel,
    string LicensePlate);
