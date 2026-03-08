namespace PartnerService.Application.Models;

public sealed record PartnerPayoutResult(
    long Id,
    string RequestKey,
    Guid PartnerUserId,
    decimal Amount,
    string Currency,
    string Status,
    DateTimeOffset RequestedAt,
    DateTimeOffset? ProcessedAt,
    string? FailureReason);
