using PartnerService.Application.Models;

namespace PartnerService.Application.Interfaces;

public interface IPartnerBookingClient
{
    Task<IReadOnlyCollection<PartnerBookingResult>> GetBookingsAsync(
        Guid partnerUserId,
        CancellationToken cancellationToken = default);
}
