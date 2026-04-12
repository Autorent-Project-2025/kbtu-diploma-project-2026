using TicketService.Application.Models;

namespace TicketService.Application.Interfaces;

public interface IPartnerCarProvisioningClient
{
    Task ProvisionPartnerCarAsync(
        ProvisionPartnerCarRequest request,
        CancellationToken cancellationToken = default);

    Task ApplyApprovedUpdateAsync(
        ApplyPartnerCarApprovedUpdateRequest request,
        CancellationToken cancellationToken = default);
}
