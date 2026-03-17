namespace AutoRent.Messaging.Contracts;

public sealed record ClientApprovedEmailRequested(
    Guid TicketId,
    string To,
    string FullName,
    string LoginEmail,
    string SetPasswordUrl);

public sealed record ClientRejectedEmailRequested(
    Guid TicketId,
    string To,
    string FullName,
    string? Reason);

public sealed record PartnerApprovedEmailRequested(
    Guid TicketId,
    string To,
    string FullName,
    string LoginEmail,
    string SetPasswordUrl);

public sealed record PartnerRejectedEmailRequested(
    Guid TicketId,
    string To,
    string FullName,
    string? Reason);

public sealed record PartnerCarApprovedEmailRequested(
    Guid TicketId,
    string To,
    string FullName,
    string CarBrand,
    string CarModel,
    string LicensePlate);

public sealed record PartnerCarRejectedEmailRequested(
    Guid TicketId,
    string To,
    string FullName,
    string CarBrand,
    string CarModel,
    string LicensePlate,
    string? Reason);
