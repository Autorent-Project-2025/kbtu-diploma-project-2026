using PartnerService.Application.Models;

namespace PartnerService.Application.Interfaces;

public interface IPartnerPaymentClient
{
    Task<PartnerWalletResult> GetWalletAsync(Guid partnerUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PartnerLedgerEntryResult>> GetLedgerAsync(Guid partnerUserId, int take = 50, CancellationToken cancellationToken = default);
    Task<PartnerPayoutResult> RequestPayoutAsync(Guid partnerUserId, decimal amount, string requestKey, CancellationToken cancellationToken = default);
    Task<PartnerPayoutResult?> GetPayoutAsync(long payoutId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PartnerPayoutResult>> GetPayoutsAsync(Guid partnerUserId, int take = 50, CancellationToken cancellationToken = default);
    Task<PartnerPayoutResult> CancelPayoutAsync(long payoutId, string? reason = null, CancellationToken cancellationToken = default);
}
