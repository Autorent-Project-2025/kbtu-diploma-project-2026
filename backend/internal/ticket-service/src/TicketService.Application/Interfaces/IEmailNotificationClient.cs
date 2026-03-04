using TicketService.Application.Models;

namespace TicketService.Application.Interfaces;

public interface IEmailNotificationClient
{
    Task SendApprovedAsync(
        SendApprovedEmailRequest request,
        CancellationToken cancellationToken = default);

    Task SendRejectedAsync(
        SendRejectedEmailRequest request,
        CancellationToken cancellationToken = default);

    Task SendPartnerApprovedAsync(
        SendPartnerApprovedEmailRequest request,
        CancellationToken cancellationToken = default);

    Task SendPartnerRejectedAsync(
        SendPartnerRejectedEmailRequest request,
        CancellationToken cancellationToken = default);

    Task SendPartnerCarApprovedAsync(
        SendPartnerCarApprovedEmailRequest request,
        CancellationToken cancellationToken = default);

    Task SendPartnerCarRejectedAsync(
        SendPartnerCarRejectedEmailRequest request,
        CancellationToken cancellationToken = default);
}
