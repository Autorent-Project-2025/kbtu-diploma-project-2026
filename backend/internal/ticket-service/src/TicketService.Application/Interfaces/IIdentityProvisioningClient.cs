using TicketService.Application.Models;

namespace TicketService.Application.Interfaces;

public interface IIdentityProvisioningClient
{
    Task<ProvisionIdentityUserResult> ProvisionUserAsync(
        ProvisionIdentityUserRequest request,
        CancellationToken cancellationToken = default);
}
