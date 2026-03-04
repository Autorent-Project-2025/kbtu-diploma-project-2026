namespace TicketService.Application.Models;

public sealed record SendPartnerCarRejectedEmailRequest(
    string To,
    string FullName,
    string CarBrand,
    string CarModel,
    string LicensePlate,
    string Reason);
