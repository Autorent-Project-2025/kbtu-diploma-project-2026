namespace PartnerService.Application.Models;

public sealed record PartnerWalletResult(
    Guid PartnerUserId,
    string Currency,
    decimal PendingAmount,
    decimal AvailableAmount,
    decimal ReservedAmount,
    DateTimeOffset UpdatedAt);
