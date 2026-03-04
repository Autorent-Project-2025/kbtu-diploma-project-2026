using TicketService.Application.Models;

namespace TicketService.Application.Interfaces;

public interface IPartnerCarProvisioningClient
{
    Task ProvisionPartnerCarAsync(
        ProvisionPartnerCarRequest request,
        CancellationToken cancellationToken = default);
}
